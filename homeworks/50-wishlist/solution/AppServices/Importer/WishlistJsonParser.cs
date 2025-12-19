using System.Text.Json;

namespace AppServices.Importer;

/// <summary>
/// Interface for parsing JSON content into objects
/// </summary>
public interface IWishlistJsonParser
{
    /// <summary>
    /// Parses JSON content into a wishlist import file DTO
    /// </summary>
    /// <param name="fileName">Name of the JSON file (for error reporting)</param>
    /// <param name="jsonContent">JSON content as string</param>
    /// <returns>Parsed wishlist import file DTO</returns>
    WishlistImportFileDto ParseJson(string fileName, string jsonContent);
}

public class WishlistParseException(string fileName, Exception? innerException)
    : Exception($"Error while parsing wishlist JSON file: {fileName}", innerException)
{
}

/// <summary>
/// Implementation for parsing JSON content into wishlist import DTOs
/// </summary>
public class WishlistJsonParser : IWishlistJsonParser
{
    public WishlistImportFileDto ParseJson(string fileName, string jsonContent)
    {
        try
        {
            return JsonSerializer.Deserialize<WishlistImportFileDto>(jsonContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                ?? throw new WishlistParseException(fileName, null);
        }
        catch (Exception ex)
        {
            throw new WishlistParseException(fileName, ex);
        }
    }
}

public record WishlistImportFileDto
{
    public WishlistHeaderImportDto Wishlist { get; set; } = new();
    public List<WishlistItemImportDto> Items { get; set; } = [];
}

public record WishlistHeaderImportDto
{
    public string Name { get; set; } = string.Empty;
    public string ParentPin { get; set; } = string.Empty;
    public string ChildPin { get; set; } = string.Empty;
}

public record WishlistItemImportDto
{
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Bought { get; set; }
}
