namespace AppServices.Importer;

public record SplitRowData(int Startnummer, string Vorname, string Nachname, int AngestrebteGesamtzeitSek, int KmNummer, int ZeitSekunden);
public record ParsedSplitData(string Description, List<SplitRowData> Rows);

public interface ISplitCsvParser
{
    ParsedSplitData ParseCsv(string csvContent);
}

public enum SplitImportError
{
    MissingDescription,
    DescriptionTooLong,
    MissingEmptyLine,
    MissingCsvHeader,
    InvalidCsvHeader,
    IncorrectColumnCount,
    InvalidStartnummer,
    MissingVorname,
    MissingNachname,
    InvalidAngestrebteGesamtzeit,
    InconsistentRunnerData,
    InvalidKmNummer,
    KmNummerNotConsecutive,
    InvalidZeit,
}

public class SplitParseException(SplitImportError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<SplitImportError, string> ErrorMessages = new()
    {
        { SplitImportError.MissingDescription, "Description (line 1) is missing or empty." },
        { SplitImportError.DescriptionTooLong, "Description (line 1) exceeds maximum length of 100 characters." },
        { SplitImportError.MissingEmptyLine, "Line 2 must be empty." },
        { SplitImportError.MissingCsvHeader, "CSV header (line 3) is missing." },
        { SplitImportError.InvalidCsvHeader, "CSV header (line 3) must be exactly: Startnummer,Vorname,Nachname,AngestrebteGesamtzeit,KmNummer,Zeit (in this order)." },
        { SplitImportError.IncorrectColumnCount, "Incorrect number of columns in data row." },
        { SplitImportError.InvalidStartnummer, "Invalid Startnummer; must be a positive integer." },
        { SplitImportError.MissingVorname, "Vorname is missing." },
        { SplitImportError.MissingNachname, "Nachname is missing." },
        { SplitImportError.InvalidAngestrebteGesamtzeit, "Invalid AngestrebteGesamtzeit; expected (H:)MM:SS with seconds 0-59." },
        { SplitImportError.InconsistentRunnerData, "Runner metadata (Vorname, Nachname, or AngestrebteGesamtzeit) are inconsistent for the same Startnummer." },
        { SplitImportError.InvalidKmNummer, "Invalid KmNummer; must be a positive integer." },
        { SplitImportError.KmNummerNotConsecutive, "KmNummern are not consecutively ascending starting from 1." },
        { SplitImportError.InvalidZeit, "Invalid Zeit; expected MM:SS with seconds 0-59 and value > 0." },
    };

    public SplitImportError ErrorCode { get; } = errorCode;
}

public class SplitCsvParser : ISplitCsvParser
{
    public ParsedSplitData ParseCsv(string csvContent)
    {
        // TODO: Implement the CSV parsing and validation logic.
        // Parse the CSV content according to the format described in the specification.
        // Throw SplitParseException with the appropriate SplitImportError for each validation failure.
        throw new NotImplementedException();
    }
}
