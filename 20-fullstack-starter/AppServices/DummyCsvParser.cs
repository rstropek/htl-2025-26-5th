namespace AppServices;

/// <summary>
/// Interface for parsing CSV content into objects
/// </summary>
public interface IDummyCsvParser
{
    /// <summary>
    /// Parses CSV content into a list of Dummy objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <returns>List of parsed Dummy objects</returns>
    IEnumerable<Dummy> ParseCsv(string csvContent);
}

/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class DummyCsvParser : IDummyCsvParser
{
    public IEnumerable<Dummy> ParseCsv(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        if (lines.Length == 0)
        {
            throw new InvalidOperationException("CSV content is empty.");
        }

        // Parse header
        var header = lines[0].Split(';');
        if (header.Length < 2 || header[0] != "Name" || header[1] != "DecimalProperty")
        {
            throw new InvalidOperationException("Invalid CSV header. Expected: Name;DecimalProperty");
        }

        var dummies = new List<Dummy>();

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var values = line.Split(';');

            if (values.Length < 2)
            {
                throw new InvalidOperationException($"Line {i + 1} has insufficient columns.");
            }

            var name = values[0].Trim();
            var decimalValueStr = values[1].Trim();

            if (!decimal.TryParse(decimalValueStr, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var decimalValue))
            {
                throw new InvalidOperationException($"Invalid decimal value '{decimalValueStr}' on line {i + 1}.");
            }

            dummies.Add(new Dummy
            {
                Name = name,
                DecimalProperty = decimalValue
            });
        }

        return dummies;
    }
}
