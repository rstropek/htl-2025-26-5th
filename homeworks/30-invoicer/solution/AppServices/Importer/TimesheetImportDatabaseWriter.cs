using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing objects to the database
/// </summary>
public interface ITimesheetImportDatabaseReaderWriter
{
    /// <summary>
    /// Clears existing time entries for a given combination of employee and date
    /// </summary>
    Task ClearDayAsync(string employeeId, DateOnly date);

    /// <summary>
    /// Writes a collection of TimeEntry objects to the database
    /// </summary>
    /// <param name="entries">TimeEntries to write</param>
    Task WriteTimeEntriesAsync(IEnumerable<TimeEntry> entries);

    Task<IEnumerable<Employee>> GetAllEmployeesAsync();

    Task<IEnumerable<Project>> GetAllProjectsAsync();

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}

/// <summary>
/// Implementation for writing objects to the database
/// </summary>
public class TimesheetImportDatabaseReaderWriter(ApplicationDataContext context) : ITimesheetImportDatabaseReaderWriter
{
    private IDbContextTransaction? transaction;

    public async Task ClearDayAsync(string employeeId, DateOnly date)
    {
        await context.TimeEntries
            .Where(te => te.Employee!.EmplyeeId == employeeId && te.Date == date)
            .ExecuteDeleteAsync();
    }

    public async Task WriteTimeEntriesAsync(IEnumerable<TimeEntry> entries)
    {
        context.TimeEntries.AddRange(entries);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync() => await context.Employees.ToListAsync();

    public async Task<IEnumerable<Project>> GetAllProjectsAsync() => await context.Projects.ToListAsync();

    public async Task BeginTransactionAsync()
    {
        transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.CommitAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }
}
