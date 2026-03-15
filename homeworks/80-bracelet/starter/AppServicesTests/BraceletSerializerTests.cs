using AppServices;

namespace AppServicesTests;

public class BraceletSerializerTests
{
    private readonly BraceletSerializer serializer = new();

    [Fact]
    public void Parse_ValidBracelet_ReturnsOkAndBracelet()
    {
        var result = serializer.Parse("A|pink|B|mint|C", out var bracelet);
        Assert.Equal(BraceletValidationResult.Ok, result);
        Assert.NotNull(bracelet);
        Assert.Equal(["A", "pink", "B", "mint", "C"], bracelet.Parts);
        Assert.Equal("A|pink|B|mint|C", bracelet.Data);
    }

    [Fact]
    public void Parse_SingleLetter_ReturnsOk()
    {
        var result = serializer.Parse("A", out var bracelet);
        Assert.Equal(BraceletValidationResult.Ok, result);
        Assert.NotNull(bracelet);
    }

    [Fact]
    public void Parse_SpecialChars_ReturnsOk()
    {
        var result = serializer.Parse("\u2665|blue|\u2605", out var bracelet);
        Assert.Equal(BraceletValidationResult.Ok, result);
        Assert.NotNull(bracelet);
    }

    [Fact]
    public void Parse_InvalidLetter_ReturnsInvalidLetterAndNullBracelet()
    {
        var result = serializer.Parse("1|pink|A", out var bracelet);
        Assert.Equal(BraceletValidationResult.InvalidLetter, result);
        Assert.Null(bracelet);
    }

    [Fact]
    public void Parse_InvalidColor_ReturnsInvalidColor()
    {
        var result = serializer.Parse("A|red|B", out var bracelet);
        Assert.Equal(BraceletValidationResult.InvalidColor, result);
        Assert.Null(bracelet);
    }

    [Fact]
    public void Parse_EndsWithSpacer_ReturnsEndsWithSpacer()
    {
        var result = serializer.Parse("A|pink", out var bracelet);
        Assert.Equal(BraceletValidationResult.EndsWithSpacer, result);
        Assert.Null(bracelet);
    }

    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        var result = serializer.Parse("", out var bracelet);
        Assert.Equal(BraceletValidationResult.Empty, result);
        Assert.Null(bracelet);
    }

    [Fact]
    public void Parse_TooManyLetters_ReturnsTooManyLetters()
    {
        var parts = new List<string>();
        for (int i = 0; i < 11; i++)
        {
            parts.Add(((char)('A' + i)).ToString());
            if (i < 10)
            {
                parts.Add("pink");
            }
        }
        var data = string.Join("|", parts);
        var result = serializer.Parse(data, out var bracelet);
        Assert.Equal(BraceletValidationResult.TooManyLetters, result);
        Assert.Null(bracelet);
    }
}
