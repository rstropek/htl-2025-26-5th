using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing objects to the database
/// </summary>
public interface IDummyImportDatabaseWriter
{
    /// <summary>
    /// Clears all existing Dummy records from the database
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// Writes a collection of Dummy objects to the database
    /// </summary>
    /// <param name="dummies">Dummies to write</param>
    Task WriteDummiesAsync(IEnumerable<Dummy> dummies);

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
public class DummyImportDatabaseWriter(ApplicationDataContext context) : IDummyImportDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public async Task ClearAllAsync()
    {
        await context.Dummies.ExecuteDeleteAsync();
    }

    public async Task WriteDummiesAsync(IEnumerable<Dummy> dummies)
    {
        await context.Dummies.AddRangeAsync(dummies);
        await context.SaveChangesAsync();
    }

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
