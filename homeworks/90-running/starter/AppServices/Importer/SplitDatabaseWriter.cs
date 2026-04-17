using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

public interface ISplitDatabaseWriter
{
    Task WriteTeilnehmerAsync(IEnumerable<Teilnehmer> teilnehmer);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class SplitDatabaseWriter(ApplicationDataContext context) : ISplitDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public async Task WriteTeilnehmerAsync(IEnumerable<Teilnehmer> teilnehmer)
    {
        context.Teilnehmer.AddRange(teilnehmer);
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
