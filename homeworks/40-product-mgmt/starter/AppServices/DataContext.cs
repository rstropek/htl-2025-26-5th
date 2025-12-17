using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AppServices;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var path = configuration["Database:path"] ?? throw new InvalidOperationException("Database path not configured.");
        var fileName = configuration["Database:fileName"] ?? throw new InvalidOperationException("Database file name not configured.");
        optionsBuilder.UseSqlite($"Data Source={path}/{fileName}");

        return new ApplicationDataContext(optionsBuilder.Options);
    }
}