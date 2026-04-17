using AppServices;
using AppServices.Importer;
using NSubstitute;
using TestInfrastructure;
using Xunit;

namespace ImporterTests;

public class DataImporterTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task SuccessfulImport_CommitsAndReturnsCount()
    {
        var fileReader = Substitute.For<IFileReader>();
        var parser = Substitute.For<ISplitCsvParser>();
        var writer = Substitute.For<ISplitDatabaseWriter>();

        var csvContent = "some content";
        fileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(csvContent);

        var rows = new List<SplitRowData>
        {
            new(1, "Anna", "Bauer", 1800, 1, 300),
            new(1, "Anna", "Bauer", 1800, 2, 310),
            new(1, "Anna", "Bauer", 1800, 3, 295),
        };
        parser.ParseCsv(csvContent).Returns(new ParsedSplitData("Test", rows));

        // Add a Laufbewerb to the DB
        var bewerb = new Laufbewerb
        {
            Name = "Import Test",
            LaufkategorieId = 1,
            Streckenlänge = 3.0m,
            Datum = new DateOnly(2026, 5, 1),
            Ort = "Test"
        };
        fixture.Context.Laufbewerbe.Add(bewerb);
        await fixture.Context.SaveChangesAsync();

        var importer = new SplitImporter(fileReader, parser, writer, fixture.Context);
        var count = await importer.ImportFromCsvAsync("test.csv", bewerb.Id, isDryRun: false);

        Assert.Equal(1, count);
        await writer.Received(1).CommitTransactionAsync();
        await writer.DidNotReceive().RollbackTransactionAsync();

        fixture.Context.Laufbewerbe.Remove(bewerb);
        await fixture.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task DryRun_RollsBack()
    {
        var fileReader = Substitute.For<IFileReader>();
        var parser = Substitute.For<ISplitCsvParser>();
        var writer = Substitute.For<ISplitDatabaseWriter>();

        var csvContent = "some content";
        fileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(csvContent);

        var rows = new List<SplitRowData>
        {
            new(1, "Max", "Muster", 1500, 1, 290),
            new(1, "Max", "Muster", 1500, 2, 295),
            new(1, "Max", "Muster", 1500, 3, 300),
        };
        parser.ParseCsv(csvContent).Returns(new ParsedSplitData("DryRun Test", rows));

        var bewerb = new Laufbewerb
        {
            Name = "Dry Run Test",
            LaufkategorieId = 1,
            Streckenlänge = 3.0m,
            Datum = new DateOnly(2026, 5, 2),
            Ort = "TestOrt"
        };
        fixture.Context.Laufbewerbe.Add(bewerb);
        await fixture.Context.SaveChangesAsync();

        var importer = new SplitImporter(fileReader, parser, writer, fixture.Context);
        await importer.ImportFromCsvAsync("test.csv", bewerb.Id, isDryRun: true);

        await writer.Received(1).RollbackTransactionAsync();
        await writer.DidNotReceive().CommitTransactionAsync();

        fixture.Context.Laufbewerbe.Remove(bewerb);
        await fixture.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task ParserError_RollsBackAndRethrows()
    {
        var fileReader = Substitute.For<IFileReader>();
        var parser = Substitute.For<ISplitCsvParser>();
        var writer = Substitute.For<ISplitDatabaseWriter>();

        fileReader.ReadAllTextAsync(Arg.Any<string>()).Returns("content");
        parser.ParseCsv(Arg.Any<string>()).Throws(new SplitParseException(SplitImportError.MissingDescription));

        var bewerb = new Laufbewerb
        {
            Name = "Parser Error Test",
            LaufkategorieId = 1,
            Streckenlänge = 5.0m,
            Datum = new DateOnly(2026, 5, 3),
            Ort = "TestOrt"
        };
        fixture.Context.Laufbewerbe.Add(bewerb);
        await fixture.Context.SaveChangesAsync();

        var importer = new SplitImporter(fileReader, parser, writer, fixture.Context);
        await Assert.ThrowsAsync<SplitParseException>(async () =>
            await importer.ImportFromCsvAsync("test.csv", bewerb.Id));

        await writer.Received(1).RollbackTransactionAsync();

        fixture.Context.Laufbewerbe.Remove(bewerb);
        await fixture.Context.SaveChangesAsync();
    }
}
