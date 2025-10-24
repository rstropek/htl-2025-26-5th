namespace AppServices.Importer;

/// <summary>
/// Interface for importing data from CSV files
/// </summary>
public interface IDummyImporter
{
    /// <summary>
    /// Imports data from a CSV file
    /// </summary>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of records imported</returns>
    Task<int> ImportFromCsvAsync(string csvFilePath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing data from CSV files
/// </summary>
public class DummyImporter(IFileReader fileReader, IDummyCsvParser csvParser, IDummyImportDatabaseWriter databaseWriter) : IDummyImporter
{
    public async Task<int> ImportFromCsvAsync(string csvFilePath, bool isDryRun = false)
    {
        await databaseWriter.BeginTransactionAsync();

        try
        {
            // Clear existing data
            await databaseWriter.ClearAllAsync();

            // Read CSV file
            var csvContent = await fileReader.ReadAllTextAsync(csvFilePath);

            // Parse CSV content
            var dummies = csvParser.ParseCsv(csvContent).ToList();

            // Write to database
            await databaseWriter.WriteDummiesAsync(dummies);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return dummies.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
