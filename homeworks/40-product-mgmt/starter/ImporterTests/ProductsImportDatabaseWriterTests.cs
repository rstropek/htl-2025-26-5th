using AppServices;
using AppServices.Importer;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace ImporterTests;

public class ProductsImportDatabaseWriterTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task ClearProductsAsync_RemovesAllProducts()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.AddRange(
                new Product { ProductCode = "BKE0001", ProductName = "Bike 1", PricePerUnit = 699.99m },
                new Product { ProductCode = "BKE0002", ProductName = "Bike 2", PricePerUnit = 1199.50m },
                new Product { ProductCode = "BKE0003", ProductName = "Bike 3", PricePerUnit = 549.00m }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.ClearProductsAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Products.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task WriteProductsAsync_CreatesNewProducts()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            // Clear existing data
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        var products = new List<Product>
        {
            new() 
            { 
                ProductCode = "BKE0001", 
                ProductName = "Mountain Bike Alpha",
                ProductDescription = "Entry-level mountain bike",
                Category = "Mountain Bikes",
                PricePerUnit = 699.99m
            }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.WriteProductsAsync(products);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var productCount = await context.Products.CountAsync();
            Assert.Equal(1, productCount);

            var product = await context.Products.FirstAsync();
            Assert.Equal("BKE0001", product.ProductCode);
            Assert.Equal("Mountain Bike Alpha", product.ProductName);
            Assert.Equal("Entry-level mountain bike", product.ProductDescription);
            Assert.Equal("Mountain Bikes", product.Category);
            Assert.Equal(699.99m, product.PricePerUnit);
        }
    }

    [Fact]
    public async Task WriteProductsAsync_MultipleProducts_CreatesAll()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        var products = new List<Product>
        {
            new() { ProductCode = "BKE0001", ProductName = "Mountain Bike Alpha", PricePerUnit = 699.99m },
            new() { ProductCode = "BKE0002", ProductName = "Road Bike Swift", PricePerUnit = 1199.50m },
            new() { ProductCode = "BKE0003", ProductName = "City Bike UrbanEase", PricePerUnit = 549.00m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.WriteProductsAsync(products);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var productCount = await context.Products.CountAsync();
            Assert.Equal(3, productCount);

            Assert.Contains(await context.Products.ToListAsync(), p => p.ProductCode == "BKE0001");
            Assert.Contains(await context.Products.ToListAsync(), p => p.ProductCode == "BKE0002");
            Assert.Contains(await context.Products.ToListAsync(), p => p.ProductCode == "BKE0003");
        }
    }

    [Fact]
    public async Task WriteProductsAsync_WithNullOptionalFields_SavesCorrectly()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        var products = new List<Product>
        {
            new() 
            { 
                ProductCode = "BKE0011", 
                ProductName = "Replacement Screws",
                ProductDescription = null,
                Category = null,
                PricePerUnit = 1299.00m
            }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.WriteProductsAsync(products);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var product = await context.Products.FirstAsync();
            Assert.Equal("BKE0011", product.ProductCode);
            Assert.Equal("Replacement Screws", product.ProductName);
            Assert.Null(product.ProductDescription);
            Assert.Null(product.Category);
            Assert.Equal(1299.00m, product.PricePerUnit);
        }
    }

    [Fact]
    public async Task TransactionMethods_CommitSucceeds()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        var products = new List<Product>
        {
            new() { ProductCode = "BKE0001", ProductName = "Mountain Bike Alpha", PricePerUnit = 699.99m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.BeginTransactionAsync();
            await writer.WriteProductsAsync(products);
            await writer.CommitTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Products.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task TransactionMethods_RollbackSucceeds()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        var products = new List<Product>
        {
            new() { ProductCode = "BKE0001", ProductName = "Mountain Bike Alpha", PricePerUnit = 699.99m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.BeginTransactionAsync();
            await writer.WriteProductsAsync(products);
            await writer.RollbackTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Products.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task ClearProductsAsync_EmptyDatabase_DoesNotThrow()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        // Act & Assert - Should not throw
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.ClearProductsAsync();
        }

        // Verify still empty
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Products.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task WriteProductsAsync_EmptyList_DoesNotThrow()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        var products = new List<Product>();

        // Act & Assert - Should not throw
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.WriteProductsAsync(products);
        }

        // Verify still empty
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Products.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task ClearProductsAsync_ThenWriteProducts_ReplacesData()
    {
        // Arrange - Add initial products
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            context.Products.AddRange(
                new Product { ProductCode = "OLD0001", ProductName = "Old Product 1", PricePerUnit = 100.00m },
                new Product { ProductCode = "OLD0002", ProductName = "Old Product 2", PricePerUnit = 200.00m }
            );
            await context.SaveChangesAsync();
        }

        var newProducts = new List<Product>
        {
            new() { ProductCode = "NEW0001", ProductName = "New Product 1", PricePerUnit = 300.00m },
            new() { ProductCode = "NEW0002", ProductName = "New Product 2", PricePerUnit = 400.00m },
            new() { ProductCode = "NEW0003", ProductName = "New Product 3", PricePerUnit = 500.00m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.ClearProductsAsync();
            await writer.WriteProductsAsync(newProducts);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var products = await context.Products.ToListAsync();
            Assert.Equal(3, products.Count);
            Assert.DoesNotContain(products, p => p.ProductCode.StartsWith("OLD"));
            Assert.Contains(products, p => p.ProductCode == "NEW0001");
            Assert.Contains(products, p => p.ProductCode == "NEW0002");
            Assert.Contains(products, p => p.ProductCode == "NEW0003");
        }
    }

    [Fact]
    public async Task TransactionMethods_MultipleOperations_CommitsAll()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            context.Products.Add(new Product { ProductCode = "OLD0001", ProductName = "Old Product", PricePerUnit = 100.00m });
            await context.SaveChangesAsync();
        }

        var newProducts = new List<Product>
        {
            new() { ProductCode = "NEW0001", ProductName = "New Product", PricePerUnit = 200.00m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.BeginTransactionAsync();
            await writer.ClearProductsAsync();
            await writer.WriteProductsAsync(newProducts);
            await writer.CommitTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var products = await context.Products.ToListAsync();
            Assert.Single(products);
            Assert.Equal("NEW0001", products[0].ProductCode);
        }
    }

    [Fact]
    public async Task TransactionMethods_MultipleOperations_RollbacksAll()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Products.RemoveRange(context.Products);
            context.Products.Add(new Product { ProductCode = "OLD0001", ProductName = "Old Product", PricePerUnit = 100.00m });
            await context.SaveChangesAsync();
        }

        var newProducts = new List<Product>
        {
            new() { ProductCode = "NEW0001", ProductName = "New Product", PricePerUnit = 200.00m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new ProductImportDatabaseWriter(context);
            await writer.BeginTransactionAsync();
            await writer.ClearProductsAsync();
            await writer.WriteProductsAsync(newProducts);
            await writer.RollbackTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var products = await context.Products.ToListAsync();
            Assert.Single(products);
            Assert.Equal("OLD0001", products[0].ProductCode); // Old product still exists
        }
    }
}
