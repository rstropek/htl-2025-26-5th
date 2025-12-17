namespace AppServices.Importer;

/*
 ⚠️ This parser must be generically usable. It is NOT specialized for products only.
 */

/// <summary>
/// Interface for parsing typed CSV files into dictionaries
/// </summary>
public interface ITypedCsvParser
{
    /// <summary>
    /// Parses file content into a list of dictionaries
    /// </summary>
    /// <param name="fileContent">File content as string</param>
    /// <returns>
    /// List of parsed dictionaries with column names as keys and parsed values as objects
    /// </returns>
    /// <exception cref="FileParseException">
    /// Thrown when file content is invalid.
    /// </exception>
    IEnumerable<Dictionary<string, object>> Parse(string fileContent);
}

/// <summary>
/// Represents all possible validation errors for the import file format.
/// </summary>
public enum ImportFileError
{
    // Header Errors
    MissingHeader,
    HeaderFormatError,              // Invalid separator in header line
    InvalidHeader,                  // Unrecognized header line format
    UnknownDataType,               // Data type not recognized
    InvalidOptionalMarker,         // Optionality marker not recognized
    
    // Data Section Errors
    MissingColumn,                 // Data row has fewer/more values than headers
    MissingQuotes,                 // String value not enclosed in quotes
    WrongDataType,                 // Value format doesn't match column type
}

public class FileParseException(ImportFileError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<ImportFileError, string> ErrorMessages = new()
    {
        { ImportFileError.MissingHeader, "No header section found before separator." },
        { ImportFileError.HeaderFormatError, "Invalid separator in header line; expected ': ' and ', '." },
        { ImportFileError.InvalidHeader, "Unrecognized header line format." },
        { ImportFileError.UnknownDataType, "Data type not recognized; expected STRING(<n>) or DECIMAL." },
        { ImportFileError.InvalidOptionalMarker, "Optionality marker not recognized; expected MANDATORY or OPTIONAL." },
        { ImportFileError.MissingColumn, "Data row has incorrect number of values compared to header." },
        { ImportFileError.MissingQuotes, "String value not enclosed in double quotes." },
        { ImportFileError.WrongDataType, "Value format doesn't match column type." },
    };

    public ImportFileError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Represents a column definition from the header section
/// </summary>
record ColumnDefinition(string Name, string DataType, int? MaxLength, bool IsMandatory);

/// <summary>
/// Implementation for parsing import file content into dictionaries
/// </summary>
public class TypedCsvParser : ITypedCsvParser
{
    /// <inheritdoc/>
    public IEnumerable<Dictionary<string, object>> Parse(string fileContent)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }
}
