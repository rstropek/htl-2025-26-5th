using Microsoft.EntityFrameworkCore;

namespace AppServices.Importer;

public interface ISplitImporter
{
    Task<int> ImportFromCsvAsync(string csvFilePath, int laufbewerbId, bool isDryRun = false);
}

public class SplitImporter(
    IFileReader fileReader,
    ISplitCsvParser csvParser,
    ISplitDatabaseWriter databaseWriter,
    ApplicationDataContext context) : ISplitImporter
{
    public async Task<int> ImportFromCsvAsync(string csvFilePath, int laufbewerbId, bool isDryRun = false)
    {
        // TODO: Implement the import logic:
        // 1. Begin a database transaction
        // 2. Read and parse the CSV file
        // 3. Validate that each runner has exactly ⌈Streckenlänge⌉ splits
        // 4. Delete existing Teilnehmer for this Laufbewerb (re-import support)
        // 5. Calculate SegmentLänge for each split (1.00 for full km, remainder for last segment)
        // 6. Create Teilnehmer with their Splits and write to database
        // 7. Commit or rollback based on isDryRun flag
        // 8. Return the number of imported Teilnehmer
        throw new NotImplementedException();
    }
}
