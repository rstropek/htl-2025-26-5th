using Importer;

namespace ImporterTests;

public class CommandLineParserTests
{
    private readonly CommandLineParser parser = new();

    [Fact]
    public void Parse_ValidArguments_ReturnsCorrectResult()
    {
        // Arrange
        var args = new[] { "test.txt" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("test.txt", result.FilePath);
        Assert.False(result.IsDryRun);
    }

    [Fact]
    public void Parse_WithDryRunFlag_ReturnsDryRunTrue()
    {
        // Arrange
        var args = new[] { "test.txt", "--dry-run" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("test.txt", result.FilePath);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_NoArguments_ThrowsArgumentException()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => parser.Parse(args));
        Assert.Contains("provide a CSV file path", exception.Message);
        Assert.Contains("Usage:", exception.Message);
    }

    [Fact]
    public void Parse_DryRunFlagInMiddle_ReturnsDryRunTrue()
    {
        // Arrange
        var args = new[] { "test.txt", "--dry-run", "extra" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("test.txt", result.FilePath);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_DryRunFlagAtEnd_ReturnsDryRunTrue()
    {
        // Arrange
        var args = new[] { "test.txt", "other", "--dry-run" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("test.txt", result.FilePath);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_WithInvalidFlag_IgnoresIt()
    {
        // Arrange
        var args = new[] { "test.txt", "--invalid-flag" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("test.txt", result.FilePath);
        Assert.False(result.IsDryRun);
    }

    [Fact]
    public void Parse_DryRunCaseSensitive_OnlyLowercaseWorks()
    {
        // Arrange
        var argsUpperCase = new[] { "test.txt", "--DRY-RUN" };
        var argsMixedCase = new[] { "test.txt", "--Dry-Run" };

        // Act
        var resultUpperCase = parser.Parse(argsUpperCase);
        var resultMixedCase = parser.Parse(argsMixedCase);

        // Assert
        Assert.False(resultUpperCase.IsDryRun);
        Assert.False(resultMixedCase.IsDryRun);
    }
}
