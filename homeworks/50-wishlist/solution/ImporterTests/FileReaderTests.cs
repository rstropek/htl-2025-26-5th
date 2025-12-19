using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class FileReaderTests
{
    [Fact]
    public async Task ReadAllTextAsync_FileExists_ReturnsContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var expectedContent = "Test content\nLine 2";
        await File.WriteAllTextAsync(tempFile, expectedContent);
        var fileReader = new FileReader();

        try
        {
            // Act
            var result = await fileReader.ReadAllTextAsync(tempFile);

            // Assert
            Assert.Equal(expectedContent, result);
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAllTextAsync_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var fileReader = new FileReader();
        var nonExistentFile = "/path/to/nonexistent/file.txt";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await fileReader.ReadAllTextAsync(nonExistentFile));
    }

    [Fact]
    public void GetAllJsonFiles_FolderExists_ReturnsOnlyJsonFiles()
    {
        // Arrange
        var fileReader = new FileReader();
        var tempDir = Directory.CreateTempSubdirectory("wishlist-import-tests-");
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "a.json"), "{}");
            File.WriteAllText(Path.Combine(tempDir.FullName, "b.json"), "{}");
            File.WriteAllText(Path.Combine(tempDir.FullName, "note.txt"), "not json");

            // Act
            var files = fileReader.GetAllJsonFiles(tempDir.FullName).ToArray();

            // Assert
            Assert.Equal(2, files.Length);
            Assert.All(files, f => Assert.EndsWith(".json", f, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void GetAllJsonFiles_FolderDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var fileReader = new FileReader();
        var nonExistentFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => fileReader.GetAllJsonFiles(nonExistentFolder).ToArray());
    }
}
