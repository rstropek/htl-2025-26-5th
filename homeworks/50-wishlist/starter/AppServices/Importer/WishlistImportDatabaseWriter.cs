using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing objects to the database
/// </summary>
public interface IWishlistImportDatabaseWriter
{
    /// <summary>
    /// Writes a Wishlist object to the database
    /// </summary>
    /// <param name="wishlist">Wishlist to write</param>
    Task WriteWishlistAsync(Wishlist wishlist);

    /// <summary>
    /// Gets or creates a GiftCategory by name
    /// </summary>
    /// <param name="categoryName">Name of the category</param>
    /// <returns>The existing or newly created GiftCategory</returns>
    Task<GiftCategory> GetOrCreateCategoryAsync(string categoryName);

    /// <summary>
    /// Checks whether a wishlist with the given name already exists.
    /// </summary>
    /// <param name="wishlistName">Wishlist name to check</param>
    /// <returns>True if a wishlist with the given name exists</returns>
    Task<bool> WishlistExistsAsync(string wishlistName);

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
public class WishlistImportDatabaseWriter(ApplicationDataContext context) : IWishlistImportDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public async Task WriteWishlistAsync(Wishlist wishlist)
    {
        await context.Wishlists.AddAsync(wishlist);
        await context.SaveChangesAsync();
    }

    public async Task<GiftCategory> GetOrCreateCategoryAsync(string categoryName)
    {
        var category = await context.GiftCategories
            .FirstOrDefaultAsync(c => c.Name == categoryName);
        category ??= new GiftCategory { Name = categoryName };
        return category;
    }

    public Task<bool> WishlistExistsAsync(string wishlistName)
        => context.Wishlists.AnyAsync(w => w.Name == wishlistName);

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
