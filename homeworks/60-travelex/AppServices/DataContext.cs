using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AppServices;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<TravelEntity> Travels => Set<TravelEntity>();
    public DbSet<TravelReimbursementEntity> TravelReimbursements => Set<TravelReimbursementEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TravelEntity>(entity =>
        {
            entity.ToTable("Travels");
            entity.Property(e => e.Mileage).HasConversion<double>().HasColumnType("REAL");
            entity.Property(e => e.PerDiem).HasConversion<double>().HasColumnType("REAL");
            entity.Property(e => e.Expenses).HasConversion<double>().HasColumnType("REAL");
            entity.HasMany(e => e.Reimbursements)
                .WithOne(e => e.Travel)
                .HasForeignKey(e => e.TravelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TravelReimbursementEntity>(entity =>
        {
            entity.ToTable("TravelReimbursements");
            entity.HasDiscriminator<TravelReimbursementType>("Type")
                .HasValue<DriveWithPrivateCarReimbursementEntity>(TravelReimbursementType.DriveWithPrivateCar)
                .HasValue<ExpenseReimbursementEntity>(TravelReimbursementType.Expense);
        });

        modelBuilder.Entity<DriveWithPrivateCarReimbursementEntity>(entity =>
        {
            entity.Property(e => e.KM).HasColumnName("KM");
        });
    }
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