namespace AppServices.Importer;

/// <summary>
/// Interface for importing data from timesheet files
/// </summary>
public interface ITimesheetImporter
{
    /// <summary>
    /// Imports data from a timesheet file
    /// </summary>
    /// <param name="csvFilePath">Path to the timesheet file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of time entry records imported</returns>
    /// <remarks>
    /// Note that this method must delete existing time entries for the same
    /// employee and date before writing new entries from the timesheet file.
    /// </remarks>
    Task<int> ImportTimesheetsAsync(string csvFilePath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing data from timesheet files
/// </summary>
public class TimesheetImporter(IFileReader fileReader, ITimesheetParser csvParser, ITimesheetImportDatabaseReaderWriter databaseWriter) : ITimesheetImporter
{
    public async Task<int> ImportTimesheetsAsync(string csvFilePath, bool isDryRun = false)
    {
        await databaseWriter.BeginTransactionAsync();

        try
        {
            // Read timesheet file
            var timesheetText = await fileReader.ReadAllTextAsync(csvFilePath);

            // Parse CSV content
            var existingEmployees = await databaseWriter.GetAllEmployeesAsync();
            var existingProjects = await databaseWriter.GetAllProjectsAsync();

            var timesheets = csvParser.ParseCsv(timesheetText, existingEmployees, existingProjects).ToList();
            // Clear existing data
            var employeeDates = timesheets.GroupBy(t => (t.Employee!.EmplyeeId, t.Date)).Select(g => g.Key);
            foreach(var (employeeId, date) in employeeDates)
            {
                await databaseWriter.ClearDayAsync(employeeId, date);
            }

            // Write to database
            await databaseWriter.WriteTimeEntriesAsync(timesheets);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return timesheets.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
