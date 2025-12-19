namespace AppServices.Importer;

/// <summary>
/// Interface for importing wishlists from JSON files
/// </summary>
public interface IWishlistImporter
{
    /// <summary>
    /// Imports data from JSON files in the specified folder
    /// </summary>
    /// <param name="jsonFolderPath">Path to the folder with JSON files</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of wishlists imported</returns>
    Task<int> ImportFromJsonAsync(string jsonFolderPath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing wishlists from JSON files
/// </summary>
public class WishlistImporter(IFileReader fileReader, IWishlistJsonParser jsonParser, IWishlistImportDatabaseWriter databaseWriter) : IWishlistImporter
{
    public async Task<int> ImportFromJsonAsync(string jsonFolderPath, bool isDryRun = false)
    {
        await databaseWriter.BeginTransactionAsync();

        var imported = 0;
        try
        {
            Dictionary<string, GiftCategory> categoryCache = [];
            foreach (var jsonFile in fileReader.GetAllJsonFiles(jsonFolderPath))
            {
                var jsonContent = await fileReader.ReadAllTextAsync(jsonFile);
                var wishlistToImport = jsonParser.ParseJson(jsonFile, jsonContent);

                if (await databaseWriter.WishlistExistsAsync(wishlistToImport.Wishlist.Name))
                {
                    continue;
                }

                var wishlist = new Wishlist
                {
                    Name = wishlistToImport.Wishlist.Name,
                    ParentPin = wishlistToImport.Wishlist.ParentPin,
                    ChildPin = wishlistToImport.Wishlist.ChildPin
                };

                foreach (var itemDto in wishlistToImport.Items)
                {
                    var category = categoryCache.GetValueOrDefault(itemDto.Category) ?? await databaseWriter.GetOrCreateCategoryAsync(itemDto.Category);
                    categoryCache[itemDto.Category] = category;
                    var item = new WishlistItem
                    {
                        ItemName = itemDto.ItemName,
                        Bought = itemDto.Bought,
                        Category = category
                    };
                    wishlist.Items.Add(item);
                }

                await databaseWriter.WriteWishlistAsync(wishlist);
                imported++;
            }

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return imported;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
