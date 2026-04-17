using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AppServices;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<Laufkategorie> Laufkategorien => Set<Laufkategorie>();
    public DbSet<Laufbewerb> Laufbewerbe => Set<Laufbewerb>();
    public DbSet<Teilnehmer> Teilnehmer => Set<Teilnehmer>();
    public DbSet<Split> Splits => Set<Split>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Laufkategorie>(entity =>
        {
            entity.Property(e => e.Bezeichnung).HasMaxLength(100);
            entity.HasData(
                new Laufkategorie { Id = 1, Bezeichnung = "Straßenlauf" },
                new Laufkategorie { Id = 2, Bezeichnung = "Crosslauf" },
                new Laufkategorie { Id = 3, Bezeichnung = "Bahnlauf" },
                new Laufkategorie { Id = 4, Bezeichnung = "Trailrun" });
        });

        modelBuilder.Entity<Laufbewerb>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Ort).HasMaxLength(100);
            entity.Property(e => e.Streckenlänge).HasConversion<double>().HasColumnType("REAL");
            entity.HasOne(e => e.Laufkategorie)
                .WithMany()
                .HasForeignKey(e => e.LaufkategorieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Teilnehmer>(entity =>
        {
            entity.Property(e => e.Vorname).HasMaxLength(100);
            entity.Property(e => e.Nachname).HasMaxLength(100);
            entity.HasOne(e => e.Laufbewerb)
                .WithMany()
                .HasForeignKey(e => e.LaufbewerbId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Split>(entity =>
        {
            entity.Property(e => e.SegmentLaenge).HasConversion<double>().HasColumnType("REAL");
            entity.HasOne(e => e.Teilnehmer)
                .WithMany(t => t.Splits)
                .HasForeignKey(e => e.TeilnehmerId)
                .OnDelete(DeleteBehavior.Cascade);
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
