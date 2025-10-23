using AppServices;

namespace ImporterTests;

public class DummyCsvParserTests
{
    private readonly DummyCsvParser parser = new();

    [Fact]
    public void ParseCsv_ValidContent_ReturnsListOfDummies()
    {
        // Arrange
        var csvContent = "Name;DecimalProperty\nTest1;10.5\nTest2;20.75";

        // Act
        var result = parser.ParseCsv(csvContent).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[0].Name);
        Assert.Equal(10.5m, result[0].DecimalProperty);
        Assert.Equal("Test2", result[1].Name);
        Assert.Equal(20.75m, result[1].DecimalProperty);
    }

    [Fact]
    public void ParseCsv_EmptyContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseCsv_InvalidHeader_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "WrongHeader;AnotherWrong\nTest1;10.5";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("Invalid CSV header", exception.Message);
    }

    [Fact]
    public void ParseCsv_InsufficientColumns_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "Name;DecimalProperty\nTest1";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("insufficient columns", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseCsv_InvalidDecimalValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "Name;DecimalProperty\nTest1;NotANumber";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("Invalid decimal value", exception.Message);
    }

    [Fact]
    public void ParseCsv_SkipsEmptyLines_ReturnsValidDummies()
    {
        // Arrange
        var csvContent = "Name;DecimalProperty\nTest1;10.5\n\n  \nTest2;20.75";

        // Act
        var result = parser.ParseCsv(csvContent).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[0].Name);
        Assert.Equal("Test2", result[1].Name);
    }

    [Fact]
    public void ParseCsv_TrimsWhitespace_ReturnsCleanData()
    {
        // Arrange
        var csvContent = "Name;DecimalProperty\n  Test1  ;  10.5  \n  Test2  ;  20.75  ";

        // Act
        var result = parser.ParseCsv(csvContent).ToList();

        // Assert
        Assert.Equal("Test1", result[0].Name);
        Assert.Equal(10.5m, result[0].DecimalProperty);
    }
}
