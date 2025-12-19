using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AppServices;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<GiftCategory> GiftCategories => Set<GiftCategory>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.ParentPin)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(e => e.ChildPin)
                .IsRequired()
                .HasMaxLength(6);
        });

        modelBuilder.Entity<GiftCategory>(entity =>
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(e => e.Wishlist)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.CategoryId)
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