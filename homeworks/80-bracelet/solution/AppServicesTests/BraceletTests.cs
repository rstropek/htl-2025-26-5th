using AppServices;

namespace AppServicesTests;

public class BraceletTests
{
    private readonly BraceletSerializer serializer = new();

    [Fact]
    public void Cost_LettersAndSpacers()
    {
        serializer.Parse("A|pink|B|mint|C", out var bracelet);
        Assert.Equal(4.0m, bracelet!.Cost);
    }

    [Fact]
    public void Cost_SingleLetter()
    {
        serializer.Parse("A", out var bracelet);
        Assert.Equal(1.0m, bracelet!.Cost);
    }

    [Fact]
    public void HasMixedColors_SingleColor_ReturnsFalse()
    {
        serializer.Parse("A|pink|B|pink|C", out var bracelet);
        Assert.False(bracelet!.HasMixedColors);
    }

    [Fact]
    public void HasMixedColors_MixedColors_ReturnsTrue()
    {
        serializer.Parse("A|pink|B|mint|C", out var bracelet);
        Assert.True(bracelet!.HasMixedColors);
    }

    [Fact]
    public void HasMixedColors_NoSpacers_ReturnsFalse()
    {
        serializer.Parse("A", out var bracelet);
        Assert.False(bracelet!.HasMixedColors);
    }
}
