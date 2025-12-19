using AppServices;
using AppServices.Importer;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace ImporterTests;

public class WishlistImportDatabaseWriterTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task WishlistExistsAsync_WhenWishlistExists_ReturnsTrue()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Wishlists.Add(new Wishlist { Name = "Existing", ParentPin = "AAAAAA", ChildPin = "BBBBBB" });
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new WishlistImportDatabaseWriter(context);
            var exists = await writer.WishlistExistsAsync("Existing");

            // Assert
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task GetOrCreateCategoryAsync_WhenCategoryExists_ReturnsExistingCategory()
    {
        // Arrange
        int existingId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var category = new GiftCategory { Name = "Toys" };
            context.GiftCategories.Add(category);
            await context.SaveChangesAsync();
            existingId = category.Id;
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new WishlistImportDatabaseWriter(context);
            var category = await writer.GetOrCreateCategoryAsync("Toys");

            // Assert
            Assert.Equal(existingId, category.Id);
            Assert.Equal("Toys", category.Name);
        }
    }

    [Fact]
    public async Task GetOrCreateCategoryAsync_WhenCategoryDoesNotExist_CategoryIsCreatedWhenUsedInWishlistGraph()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new WishlistImportDatabaseWriter(context);
            var category = await writer.GetOrCreateCategoryAsync("Books");

            var wishlist = new Wishlist
            {
                Name = "W1",
                ParentPin = "AAAAAA",
                ChildPin = "BBBBBB",
                Items =
                [
                    new WishlistItem { ItemName = "Some Book", Bought = false, Category = category }
                ]
            };

            await writer.WriteWishlistAsync(wishlist);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            Assert.True(await context.GiftCategories.AnyAsync(c => c.Name == "Books"));
            Assert.True(await context.Wishlists.AnyAsync(w => w.Name == "W1"));
        }
    }

    [Fact]
    public async Task TransactionMethods_CommitPersistsChanges()
    {
        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new WishlistImportDatabaseWriter(context);
            await writer.BeginTransactionAsync();

            await writer.WriteWishlistAsync(new Wishlist { Name = "Commit", ParentPin = "AAAAAA", ChildPin = "BBBBBB" });

            await writer.CommitTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            Assert.True(await context.Wishlists.AnyAsync(w => w.Name == "Commit"));
        }
    }

    [Fact]
    public async Task TransactionMethods_RollbackDiscardsChanges()
    {
        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new WishlistImportDatabaseWriter(context);
            await writer.BeginTransactionAsync();

            await writer.WriteWishlistAsync(new Wishlist { Name = "Rollback", ParentPin = "AAAAAA", ChildPin = "BBBBBB" });

            await writer.RollbackTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            Assert.False(await context.Wishlists.AnyAsync(w => w.Name == "Rollback"));
        }
    }
}

