using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CashRegister.Data;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptLine> ReceiptLines => Set<ReceiptLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal handling for SQLite
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasConversion<double>();

        modelBuilder.Entity<Receipt>()
            .Property(r => r.TotalPrice)
            .HasConversion<double>();

        modelBuilder.Entity<ReceiptLine>()
            .Property(rl => rl.TotalPrice)
            .HasConversion<double>();
    }
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();

        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = configurationBuilder.Build();
            
        optionsBuilder.UseSqlite(configuration.GetConnectionString("CashRegister"));
        return new ApplicationDataContext(optionsBuilder.Options);
    }
}