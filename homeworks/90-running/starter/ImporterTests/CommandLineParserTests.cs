using Importer;
using Xunit;

namespace ImporterTests;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_ValidArgs_ReturnsCorrectResult()
    {
        var args = new[] { "splits.csv", "--laufbewerb-id", "3" };
        var result = CommandLineParser.Parse(args);
        Assert.Equal("splits.csv", result.CsvFilePath);
        Assert.Equal(3, result.LaufbewerbId);
        Assert.False(result.IsDryRun);
    }

    [Fact]
    public void Parse_WithDryRun_SetsDryRun()
    {
        var args = new[] { "splits.csv", "--laufbewerb-id", "3", "--dry-run" };
        var result = CommandLineParser.Parse(args);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_DryRunBeforeId_Works()
    {
        var args = new[] { "splits.csv", "--dry-run", "--laufbewerb-id", "5" };
        var result = CommandLineParser.Parse(args);
        Assert.Equal(5, result.LaufbewerbId);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_MissingLaufbewerbId_Throws()
    {
        var args = new[] { "splits.csv" };
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }

    [Fact]
    public void Parse_LaufbewerbIdWithoutValue_Throws()
    {
        var args = new[] { "splits.csv", "--laufbewerb-id" };
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }

    [Fact]
    public void Parse_LaufbewerbIdNotInteger_Throws()
    {
        var args = new[] { "splits.csv", "--laufbewerb-id", "abc" };
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }

    [Fact]
    public void Parse_WrongCase_Throws()
    {
        var args = new[] { "splits.csv", "--Laufbewerb-Id", "3" };
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
    }

    [Fact]
    public void Parse_EmptyArgs_Throws()
    {
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse([]));
    }
}
