namespace AppServices;

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
}
