using AppServices;
using Importer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Build the host with dependency injection
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Configure services
ConfigureServices(builder.Services, builder.Configuration);

var host = builder.Build();

try
{
    // Parse command line arguments
    var parser = new CommandLineParser();
    var parsedArgs = parser.Parse(args);

    // Validate file exists
    if (!File.Exists(parsedArgs.CsvFilePath))
    {
        Console.Error.WriteLine($"Error: File '{parsedArgs.CsvFilePath}' not found.");
        return 1;
    }

    // Get the importer service from DI container
    var importer = host.Services.GetRequiredService<IDummyImporter>();

    // Perform the import
    var importedCount = await importer.ImportFromCsvAsync(parsedArgs.CsvFilePath, parsedArgs.IsDryRun);

    Console.WriteLine($"\nSuccessfully imported {importedCount} record(s).");

    if (parsedArgs.IsDryRun)
    {
        Console.WriteLine("Dry-run mode: Transaction was rolled back.");
    }
    else
    {
        Console.WriteLine("Transaction committed.");
    }

    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\nError occurred: {ex.Message}");
    Console.Error.WriteLine("Import failed.");
    return 1;
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Register database context
    var path = configuration["Database:path"] ?? throw new InvalidOperationException("Database path not configured.");
    var fileName = configuration["Database:fileName"] ?? throw new InvalidOperationException("Database file name not configured.");
    var connectionString = $"Data Source={path}/{fileName}";

    services.AddDbContext<ApplicationDataContext>(options =>
        options.UseSqlite(connectionString));

    // Register application services
    services.AddScoped<IFileReader, FileReader>();
    services.AddScoped<IDummyCsvParser, DummyCsvParser>();
    services.AddScoped<IDummyImportDatabaseWriter, DummyImportDatabaseWriter>();
    services.AddScoped<IDummyImporter, DummyImporter>();
}
