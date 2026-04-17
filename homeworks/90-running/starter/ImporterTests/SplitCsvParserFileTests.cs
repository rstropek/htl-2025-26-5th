using AppServices.Importer;
using Xunit;

namespace ImporterTests;

public class SplitCsvParserFileTests
{
    private static readonly SplitCsvParser Parser = new();

    private static string ReadTestFile(string relativePath) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, relativePath));

    [Fact]
    public void MissingDescription_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/missing_description.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.MissingDescription, ex.ErrorCode);
    }

    [Fact]
    public void DescriptionTooLong_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/description_too_long.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.DescriptionTooLong, ex.ErrorCode);
    }

    [Fact]
    public void MissingEmptyLine_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/missing_empty_line.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.MissingEmptyLine, ex.ErrorCode);
    }

    [Fact]
    public void InvalidHeader_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/invalid_header.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.InvalidCsvHeader, ex.ErrorCode);
    }

    [Fact]
    public void WrongColumnOrder_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/wrong_column_order.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.InvalidCsvHeader, ex.ErrorCode);
    }

    [Fact]
    public void WrongColumnCount_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/wrong_column_count.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.IncorrectColumnCount, ex.ErrorCode);
    }

    [Fact]
    public void InvalidStartnummer_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/invalid_startnummer.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.InvalidStartnummer, ex.ErrorCode);
    }

    [Fact]
    public void MissingVorname_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/missing_vorname.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.MissingVorname, ex.ErrorCode);
    }

    [Fact]
    public void MissingNachname_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/missing_nachname.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.MissingNachname, ex.ErrorCode);
    }

    [Fact]
    public void InvalidZielzeit_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/invalid_zielzeit.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.InvalidAngestrebteGesamtzeit, ex.ErrorCode);
    }

    [Fact]
    public void InconsistentRunnerData_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/inconsistent_runner_data.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.InconsistentRunnerData, ex.ErrorCode);
    }

    [Fact]
    public void KmNotConsecutive_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/km_not_consecutive.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.KmNummerNotConsecutive, ex.ErrorCode);
    }

    [Fact]
    public void TooManyKmSplits_ParsesSuccessfully()
    {
        // Split count validation is done in the importer, not the parser
        var content = ReadTestFile("invalid/too_many_km_splits.csv");
        var result = Parser.ParseCsv(content);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Rows);
    }

    [Fact]
    public void InvalidSplitZeit_ThrowsCorrectError()
    {
        var content = ReadTestFile("invalid/invalid_split_zeit.csv");
        var ex = Assert.Throws<SplitParseException>(() => Parser.ParseCsv(content));
        Assert.Equal(SplitImportError.InvalidZeit, ex.ErrorCode);
    }

    [Fact]
    public void Valid_5km_ParsesCorrectly()
    {
        var content = ReadTestFile("valid/5km_stadtlauf_wien_2026.csv");
        var result = Parser.ParseCsv(content);
        Assert.NotNull(result);
        Assert.Equal(15, result.Rows.Count); // 3 runners * 5 splits
        Assert.Equal(3, result.Rows.Select(r => r.Startnummer).Distinct().Count());
    }

    [Fact]
    public void Valid_10_5km_ParsesCorrectly()
    {
        var content = ReadTestFile("valid/10_5km_donaulauf_linz_2026.csv");
        var result = Parser.ParseCsv(content);
        Assert.NotNull(result);
        Assert.Equal(22, result.Rows.Count); // 2 runners * 11 splits
    }

    [Fact]
    public void Valid_3km_ParsesCorrectly()
    {
        var content = ReadTestFile("valid/3km_fruehjahrslauf_2026.csv");
        var result = Parser.ParseCsv(content);
        Assert.NotNull(result);
        Assert.Equal(3, result.Rows.Count); // 1 runner * 3 splits
    }
}
