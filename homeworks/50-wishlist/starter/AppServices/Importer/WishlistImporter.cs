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
        // TODO: Implement import logic

        throw new NotImplementedException();
    }
}
