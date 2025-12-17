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
        var lines = fileContent.Split(["\r\n", "\n"], StringSplitOptions.None)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        
        var lineIndex = 0;

        if (lines.All(l => l != "---")) 
        {
            throw new FileParseException(ImportFileError.MissingHeader);
        }

        // Parse header section
        var columns = ParseHeaderSection(lines, ref lineIndex);

        if (columns.Count == 0)
        {
            throw new FileParseException(ImportFileError.MissingHeader);
        }

        // Expect separator line
        if (lineIndex >= lines.Length || lines[lineIndex] != "---")
        {
            throw new FileParseException(ImportFileError.MissingHeader);
        }
        lineIndex++;

        // Parse data section
        var records = new List<Dictionary<string, object>>();
        while (lineIndex < lines.Length)
        {
            var record = ParseDataRow(lines[lineIndex], columns);
            records.Add(record);
            lineIndex++;
        }

        return records;
    }

    private static List<ColumnDefinition> ParseHeaderSection(string[] lines, ref int lineIndex)
    {
        var columns = new List<ColumnDefinition>();

        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];

            // Check if we've reached the separator
            if (line == "---")
            {
                break;
            }

            // Parse header line: <column-name>: <data-type>, <optionality>
            if (!line.Contains(": "))
            {
                throw new FileParseException(ImportFileError.HeaderFormatError);
            }

            var colonIndex = line.IndexOf(": ", StringComparison.Ordinal);
            var columnName = line[..colonIndex];
            var rest = line[(colonIndex + 2)..];

            if (!rest.Contains(", "))
            {
                throw new FileParseException(ImportFileError.HeaderFormatError);
            }

            var commaIndex = rest.IndexOf(", ", StringComparison.Ordinal);
            var dataTypePart = rest[..commaIndex];
            var optionalityPart = rest[(commaIndex + 2)..];

            // Parse data type
            string dataType;
            int? maxLength = null;

            if (dataTypePart.StartsWith("STRING(", StringComparison.Ordinal) && dataTypePart.EndsWith(")"))
            {
                dataType = "STRING";
                var lengthStr = dataTypePart[7..^1];
                if (!int.TryParse(lengthStr, out var length))
                {
                    throw new FileParseException(ImportFileError.UnknownDataType);
                }
                maxLength = length;
            }
            else if (dataTypePart == "DECIMAL")
            {
                dataType = "DECIMAL";
            }
            else
            {
                throw new FileParseException(ImportFileError.UnknownDataType);
            }

            // Parse optionality
            bool isMandatory;
            if (optionalityPart == "MANDATORY")
            {
                isMandatory = true;
            }
            else if (optionalityPart == "OPTIONAL")
            {
                isMandatory = false;
            }
            else
            {
                throw new FileParseException(ImportFileError.InvalidOptionalMarker);
            }

            columns.Add(new ColumnDefinition(columnName, dataType, maxLength, isMandatory));
            lineIndex++;
        }

        return columns;
    }

    private static Dictionary<string, object> ParseDataRow(string line, List<ColumnDefinition> columns)
    {
        // Parse comma-delimited values, respecting quoted strings
        var values = ParseCsvLine(line);

        if (values.Count != columns.Count)
        {
            throw new FileParseException(ImportFileError.MissingColumn);
        }

        var record = new Dictionary<string, object>();

        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            var value = values[i];

            // Check mandatory fields
            if (column.IsMandatory && string.IsNullOrEmpty(value))
            {
                throw new FileParseException(ImportFileError.MissingColumn);
            }

            // Parse and store value based on data type
            if (column.DataType == "STRING")
            {
                if (string.IsNullOrEmpty(value))
                {
                    record[column.Name] = null!;
                }
                else
                {
                    record[column.Name] = ParseStringValue(value, column);
                }
            }
            else if (column.DataType == "DECIMAL")
            {
                record[column.Name] = ParseDecimalValue(value, column);
            }
        }

        return record;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = "";
        var insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                insideQuotes = !insideQuotes;
                currentValue += ch;
            }
            else if (ch == ',' && !insideQuotes)
            {
                values.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += ch;
            }
        }

        values.Add(currentValue);
        return values;
    }

    private static string ParseStringValue(string value, ColumnDefinition column)
    {
        if (!value.StartsWith('"') || !value.EndsWith('"'))
        {
            throw new FileParseException(ImportFileError.MissingQuotes);
        }

        var unquoted = value[1..^1];

        if (column.MaxLength.HasValue && unquoted.Length > column.MaxLength.Value)
        {
            throw new FileParseException(ImportFileError.WrongDataType);
        }

        return unquoted;
    }

    private static decimal ParseDecimalValue(string value, ColumnDefinition column)
    {
        if (value.StartsWith('"') || value.EndsWith('"'))
        {
            throw new FileParseException(ImportFileError.WrongDataType);
        }

        if (!decimal.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var decimalValue))
        {
            throw new FileParseException(ImportFileError.WrongDataType);
        }

        return decimalValue;
    }
}
