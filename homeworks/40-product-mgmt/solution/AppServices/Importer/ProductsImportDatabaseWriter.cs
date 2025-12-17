using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing product objects to the database
/// </summary>
public interface IProductImportDatabaseWriter
{
    /// <summary>
    /// Clears all existing products from the database
    /// </summary>
    Task ClearProductsAsync();

    /// <summary>
    /// Writes a collection of Product objects to the database
    /// </summary>
    /// <param name="products">Products to write</param>
    Task WriteProductsAsync(IEnumerable<Product> products);

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
/// Implementation for writing product objects to the database
/// </summary>
public class ProductImportDatabaseWriter(ApplicationDataContext context) : IProductImportDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public async Task ClearProductsAsync()
    {
        await context.Products.ExecuteDeleteAsync();
    }

    public async Task WriteProductsAsync(IEnumerable<Product> products)
    {
        context.Products.AddRange(products);
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
