using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TvGuide.Data;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options)
    : DbContext(options)
{
    public DbSet<RecordingJob> RecordingJobs => Set<RecordingJob>();
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();

        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = configurationBuilder.Build();
            
        optionsBuilder.UseSqlite(configuration.GetConnectionString("TvGuide"));
        return new ApplicationDataContext(optionsBuilder.Options);
    }
}