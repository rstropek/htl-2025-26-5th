namespace Importer;

/// <summary>
/// Result of command line parsing
/// </summary>
public record CommandLineArgs(string CsvFilePath, bool IsDryRun);

/// <summary>
/// Parser for command line arguments
/// </summary>
public class CommandLineParser
{
    public CommandLineArgs Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please provide a CSV file path as a command line argument.\nUsage: Importer <csv-file-path> [--dry-run]");
        }

        var csvFilePath = args[0];
        var isDryRun = args.Any(arg => arg == "--dry-run");

        return new CommandLineArgs(csvFilePath, isDryRun);
    }
}
