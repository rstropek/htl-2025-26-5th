using AppServices;
using Microsoft.EntityFrameworkCore;

// Parse command line arguments
if (args.Length == 0)
{
    Console.Error.WriteLine("Error: Please provide a CSV file path as a command line argument.");
    Console.Error.WriteLine("Usage: Importer <csv-file-path> [--dry-run]");
    return 1;
}

var csvFilePath = args[0];
var isDryRun = args.Length > 1 && args[1] == "--dry-run";

// Validate file exists
if (!File.Exists(csvFilePath))
{
    Console.Error.WriteLine($"Error: File '{csvFilePath}' not found.");
    return 1;
}

var contextFactory = new ApplicationDataContextFactory();
using var context = contextFactory.CreateDbContext([]);

// Start a transaction
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    // Clear existing data
    await context.Dummies.ExecuteDeleteAsync();

    // Read and parse CSV file
    var lines = await File.ReadAllLinesAsync(csvFilePath);
    
    if (lines.Length == 0)
    {
        Console.Error.WriteLine("Error: CSV file is empty.");
        await transaction.RollbackAsync();
        return 1;
    }

    // Parse header
    var header = lines[0].Split(';');
    if (header.Length < 2 || header[0] != "Name" || header[1] != "DecimalProperty")
    {
        Console.Error.WriteLine("Error: Invalid CSV header. Expected: Name;DecimalProperty");
        await transaction.RollbackAsync();
        return 1;
    }

    // Parse and import data rows
    var importedCount = 0;
    for (int i = 1; i < lines.Length; i++)
    {
        var line = lines[i].Trim();
        if (string.IsNullOrWhiteSpace(line))
        {
            continue; // Skip empty lines
        }

        var values = line.Split(';');
        if (values.Length < 2)
        {
            Console.Error.WriteLine($"Warning: Line {i + 1} has insufficient columns. Skipping.");
            continue;
        }

        var name = values[0].Trim();
        var decimalValueStr = values[1].Trim();

        if (!decimal.TryParse(decimalValueStr, out var decimalValue))
        {
            Console.Error.WriteLine($"Error: Invalid decimal value '{decimalValueStr}' on line {i + 1}.");
            await transaction.RollbackAsync();
            return 1;
        }

        var dummy = new Dummy
        {
            Name = name,
            DecimalProperty = decimalValue
        };

        context.Dummies.Add(dummy);
        importedCount++;
        Console.WriteLine($"Imported: {name} - {decimalValue}");
    }

    // Save changes
    await context.SaveChangesAsync();

    Console.WriteLine($"\nSuccessfully imported {importedCount} record(s).");

    // Rollback if dry-run or if explicitly requested
    if (isDryRun)
    {
        Console.WriteLine("Dry-run mode: Rolling back transaction.");
        await transaction.RollbackAsync();
    }
    else
    {
        await transaction.CommitAsync();
        Console.WriteLine("Transaction committed.");
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\nError occurred: {ex.Message}");
    Console.Error.WriteLine("Rolling back transaction.");
    await transaction.RollbackAsync();
    return 1;
}
