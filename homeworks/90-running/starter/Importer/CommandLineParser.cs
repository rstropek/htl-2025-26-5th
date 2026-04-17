namespace Importer;

public record CommandLineArgs(string CsvFilePath, int LaufbewerbId, bool IsDryRun);

public class CommandLineParser
{
    public static CommandLineArgs Parse(string[] args)
    {
        // TODO: Parse command line arguments:
        // - First argument: CSV file path
        // - --laufbewerb-id <id>: Required, must be a positive integer
        // - --dry-run: Optional flag
        // Throw ArgumentException with a descriptive message if arguments are invalid.
        throw new NotImplementedException();
    }
}
