namespace AppServices.Importer;

public enum ProductConversionError
{
    MissingRequiredField,
    WrongDataType,
}

/// <summary>
/// Exception thrown when product data cannot be converted to a Product entity
/// </summary>
public class ProductConversionException(ProductConversionError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown product conversion error.")
{
    private static readonly Dictionary<ProductConversionError, string> ErrorMessages = new()
    {
        { ProductConversionError.MissingRequiredField, "Required field is missing in the parsed data." },
        { ProductConversionError.WrongDataType, "Field has wrong data type in the parsed data." },
    };

    public ProductConversionError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Interface for importing data from product files
/// </summary>
public interface IProductImporter
{
    /// <summary>
    /// Imports data from a product file
    /// </summary>
    /// <param name="filePath">Path to the product file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of product records imported</returns>
    /// <remarks>
    /// Uses the <see cref="ITypedCsvParser"/> internally to parse the file content.
    /// Methodes of <see cref="IProductImportDatabaseWriter"/> are used to write data to the database.
    /// If no records are parsed from the file, the database is not modified. In that case,
    /// no methods of <see cref="IProductImportDatabaseWriter"/> are called.
    /// </remarks>
    /// <exception cref="ProductConversionException">
    /// Thrown when required fields are missing in the parsed data
    /// </exception>
    Task<int> ImportProductsAsync(string filePath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing data from product files
/// </summary>
public class ProductImporter(IFileReader fileReader, ITypedCsvParser parser, IProductImportDatabaseWriter databaseWriter) : IProductImporter
{
    private static readonly string[] RequiredColumns = ["ProductCode", "ProductName", "PricePerUnit"];

    /// <inheritdoc/>    
    public async Task<int> ImportProductsAsync(string filePath, bool isDryRun = false)
    {
        // Read product file
        var fileContent = await fileReader.ReadAllTextAsync(filePath);

        // Parse file content to dictionaries
        var records = parser.Parse(fileContent).ToList();

        // If no records were parsed, return early without touching the database
        if (records.Count == 0) { return 0; }

        await databaseWriter.BeginTransactionAsync();

        try
        {
            // Convert dictionaries to Product objects
            var products = records.Select(ConvertToProduct).ToList();

            // Clear existing products and write new ones
            await databaseWriter.ClearProductsAsync();
            await databaseWriter.WriteProductsAsync(products);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return products.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }

    private static Product ConvertToProduct(Dictionary<string, object> record)
    {
        // Validate that all required columns are present
        foreach (var requiredColumn in RequiredColumns)
        {
            if (!record.ContainsKey(requiredColumn))
            {
                throw new ProductConversionException(ProductConversionError.MissingRequiredField);
            }
        }

        // Check data types
        if (record["ProductCode"] is not string ||
            record["ProductName"] is not string ||
            (record.TryGetValue("ProductDescription", out object? productDescription) && productDescription is not null && productDescription is not string) ||
            (record.TryGetValue("Category", out object? category) && category is not null && category is not string) ||
            record["PricePerUnit"] is not decimal)
        {
            throw new ProductConversionException(ProductConversionError.WrongDataType);
        }

        return new Product
        {
            ProductCode = (string)record["ProductCode"],
            ProductName = (string)record["ProductName"],
            ProductDescription = productDescription is not null ? (string?)productDescription : null,
            Category = category is not null ? (string?)category : null,
            PricePerUnit = (decimal)record["PricePerUnit"]
        };
    }
}
