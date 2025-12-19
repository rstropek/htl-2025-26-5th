using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class WishlistImporterTests
{
    private readonly IFileReader fileReader;
    private readonly IWishlistJsonParser jsonParser;
    private readonly IWishlistImportDatabaseWriter databaseWriter;
    private readonly WishlistImporter importer;

    public WishlistImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        jsonParser = Substitute.For<IWishlistJsonParser>();
        databaseWriter = Substitute.For<IWishlistImportDatabaseWriter>();
        importer = new WishlistImporter(fileReader, jsonParser, databaseWriter);
    }

    /// <summary>
    /// Verifies that a normal import writes all non-conflicting wishlists, commits the transaction,
    /// and caches categories so the category lookup is only performed once per distinct category name.
    /// </summary>
    [Fact]
    public async Task ImportFromJsonAsync_SuccessfulImport_CommitsAndReturnsCount()
    {
        // Arrange
        var folder = "/json";
        var files = new[] { "/json/a.json", "/json/b.json" };
        fileReader.GetAllJsonFiles(folder).Returns(files);

        fileReader.ReadAllTextAsync(files[0]).Returns("{ } A");
        fileReader.ReadAllTextAsync(files[1]).Returns("{ } B");

        jsonParser.ParseJson(files[0], "{ } A").Returns(new WishlistImportFileDto
        {
            Wishlist = new WishlistHeaderImportDto { Name = "A", ParentPin = "AAAAAA", ChildPin = "CCCCCC" },
            Items =
            [
                new WishlistItemImportDto { ItemName = "Lego", Category = "Toys", Bought = false }
            ]
        });
        jsonParser.ParseJson(files[1], "{ } B").Returns(new WishlistImportFileDto
        {
            Wishlist = new WishlistHeaderImportDto { Name = "B", ParentPin = "BBBBBB", ChildPin = "DDDDDD" },
            Items =
            [
                new WishlistItemImportDto { ItemName = "Car", Category = "Toys", Bought = true }
            ]
        });

        databaseWriter.WishlistExistsAsync(Arg.Any<string>()).Returns(false);

        var toys = new GiftCategory { Name = "Toys" };
        databaseWriter.GetOrCreateCategoryAsync("Toys").Returns(toys);

        // Act
        var imported = await importer.ImportFromJsonAsync(folder, isDryRun: false);

        // Assert
        Assert.Equal(2, imported);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(2).WriteWishlistAsync(Arg.Any<Wishlist>());
        await databaseWriter.Received(1).CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();

        // Category should be cached across wishlists
        await databaseWriter.Received(1).GetOrCreateCategoryAsync("Toys");
    }

    /// <summary>
    /// Verifies that dry-run mode still processes the files and returns the number of would-be imports,
    /// but rolls back the transaction instead of committing.
    /// </summary>
    [Fact]
    public async Task ImportFromJsonAsync_DryRun_RollsBackAndReturnsCount()
    {
        // Arrange
        var folder = "/json";
        var file = "/json/a.json";
        fileReader.GetAllJsonFiles(folder).Returns([file]);
        fileReader.ReadAllTextAsync(file).Returns("{ } A");
        jsonParser.ParseJson(file, "{ } A").Returns(new WishlistImportFileDto
        {
            Wishlist = new WishlistHeaderImportDto { Name = "A", ParentPin = "AAAAAA", ChildPin = "CCCCCC" },
            Items =
            [
                new WishlistItemImportDto { ItemName = "Lego", Category = "Toys", Bought = false }
            ]
        });
        databaseWriter.WishlistExistsAsync("A").Returns(false);
        databaseWriter.GetOrCreateCategoryAsync("Toys").Returns(new GiftCategory { Name = "Toys" });

        // Act
        var imported = await importer.ImportFromJsonAsync(folder, isDryRun: true);

        // Assert
        Assert.Equal(1, imported);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    /// <summary>
    /// Verifies that existing wishlists (by name) are skipped, while non-existing ones are imported,
    /// and that only the non-conflicting wishlist is written.
    /// </summary>
    [Fact]
    public async Task ImportFromJsonAsync_WishlistAlreadyExists_SkipsWishlist()
    {
        // Arrange
        var folder = "/json";
        var files = new[] { "/json/a.json", "/json/b.json" };
        fileReader.GetAllJsonFiles(folder).Returns(files);
        fileReader.ReadAllTextAsync(Arg.Any<string>()).Returns("{ }");

        jsonParser.ParseJson(files[0], "{ }").Returns(new WishlistImportFileDto
        {
            Wishlist = new WishlistHeaderImportDto { Name = "Existing", ParentPin = "AAAAAA", ChildPin = "CCCCCC" },
            Items = []
        });
        jsonParser.ParseJson(files[1], "{ }").Returns(new WishlistImportFileDto
        {
            Wishlist = new WishlistHeaderImportDto { Name = "New", ParentPin = "BBBBBB", ChildPin = "DDDDDD" },
            Items = []
        });

        databaseWriter.WishlistExistsAsync("Existing").Returns(true);
        databaseWriter.WishlistExistsAsync("New").Returns(false);

        // Act
        var imported = await importer.ImportFromJsonAsync(folder, isDryRun: false);

        // Assert
        Assert.Equal(1, imported);
        await databaseWriter.Received(1).WriteWishlistAsync(Arg.Is<Wishlist>(w => w.Name == "New"));
        await databaseWriter.DidNotReceive().WriteWishlistAsync(Arg.Is<Wishlist>(w => w.Name == "Existing"));
    }

    /// <summary>
    /// Verifies that if any exception occurs during import (e.g. JSON parsing fails),
    /// the importer rolls back the transaction and rethrows the exception.
    /// </summary>
    [Fact]
    public async Task ImportFromJsonAsync_WhenExceptionOccurs_RollsBackAndRethrows()
    {
        // Arrange
        var folder = "/json";
        var files = new[] { "/json/a.json", "/json/b.json" };
        fileReader.GetAllJsonFiles(folder).Returns(files);

        fileReader.ReadAllTextAsync(files[0]).Returns("{ } A");
        fileReader.ReadAllTextAsync(files[1]).Returns("{ } B");

        jsonParser.ParseJson(files[0], "{ } A").Returns(new WishlistImportFileDto
        {
            Wishlist = new WishlistHeaderImportDto { Name = "A", ParentPin = "AAAAAA", ChildPin = "CCCCCC" },
            Items = []
        });

        var expected = new InvalidOperationException("boom");
        jsonParser.ParseJson(files[1], "{ } B").Throws(expected);

        databaseWriter.WishlistExistsAsync(Arg.Any<string>()).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => importer.ImportFromJsonAsync(folder, isDryRun: false));
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }
}
