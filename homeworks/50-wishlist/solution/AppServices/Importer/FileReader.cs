namespace AppServices.Importer;

/// <summary>
/// Interface for reading file content
/// </summary>
public interface IFileReader
{
    /// <summary>
    /// Reads all text from a file asynchronously
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>File content as string</returns>
    Task<string> ReadAllTextAsync(string filePath);

    /// <summary>
    /// Gets all JSON files in a folder
    /// </summary>
    /// <param name="folderPath">Path to the folder</param>
    /// <returns>Array of JSON file paths</returns>
    IEnumerable<string> GetAllJsonFiles(string folderPath);
}

/// <summary>
/// Implementation for reading file content
/// </summary>
public class FileReader : IFileReader
{
    public async Task<string> ReadAllTextAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File '{filePath}' not found.", filePath);
        }

        return await File.ReadAllTextAsync(filePath);
    }

    public IEnumerable<string> GetAllJsonFiles(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Directory '{folderPath}' not found.");
        }

        return Directory.GetFiles(folderPath, "*.json");
    }
}
