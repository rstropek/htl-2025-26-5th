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
}
