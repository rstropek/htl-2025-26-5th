using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

public class DatabaseTestsWithClassFixture(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CanAddAndRetrieveDummy()
    {
        // Arrange & Act
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new Dummy 
            { 
                Name = "Test Dummy", 
                DecimalProperty = 42.5m 
            };
            context.Dummies.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.Dummies.FindAsync(dummyId);
            Assert.NotNull(dummy);
            Assert.Equal("Test Dummy", dummy.Name);
            Assert.Equal(42.5m, dummy.DecimalProperty);
        }
    }

    [Fact]
    public async Task CanUpdateDummy()
    {
        // Arrange
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new Dummy 
            { 
                Name = "Original Name", 
                DecimalProperty = 10.0m 
            };
            context.Dummies.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.Dummies.FindAsync(dummyId);
            Assert.NotNull(dummy);
            dummy.Name = "Updated Name";
            dummy.DecimalProperty = 20.5m;
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.Dummies.FindAsync(dummyId);
            Assert.NotNull(dummy);
            Assert.Equal("Updated Name", dummy.Name);
            Assert.Equal(20.5m, dummy.DecimalProperty);
        }
    }

    [Fact]
    public async Task CanDeleteDummy()
    {
        // Arrange
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new Dummy 
            { 
                Name = "To Delete", 
                DecimalProperty = 5.0m 
            };
            context.Dummies.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.Dummies.FindAsync(dummyId);
            Assert.NotNull(dummy);
            context.Dummies.Remove(dummy);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.Dummies.FindAsync(dummyId);
            Assert.Null(dummy);
        }
    }

    [Fact]
    public async Task CanQueryMultipleDummies()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Dummies.AddRange(
                new Dummy { Name = "Query Test 1", DecimalProperty = 10.0m },
                new Dummy { Name = "Query Test 2", DecimalProperty = 20.0m },
                new Dummy { Name = "Query Test 3", DecimalProperty = 30.0m }
            );
            await context.SaveChangesAsync();
        }

        // Act & Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummies = await context.Dummies
                .Where(d => d.Name.StartsWith("Query Test") && d.DecimalProperty >= 15.0m)
                .OrderBy(d => d.Name)
                .ToListAsync();

            Assert.Equal(2, dummies.Count);
            Assert.Equal("Query Test 2", dummies[0].Name);
            Assert.Equal("Query Test 3", dummies[1].Name);
        }
    }

    [Fact]
    public async Task DecimalPropertyStoresCorrectly()
    {
        // Arrange & Act
        int dummyId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = new Dummy 
            { 
                Name = "Decimal Test", 
                DecimalProperty = 123.456m 
            };
            context.Dummies.Add(dummy);
            await context.SaveChangesAsync();
            dummyId = dummy.Id;
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var dummy = await context.Dummies.FindAsync(dummyId);
            Assert.NotNull(dummy);
            // Note: Due to the REAL conversion in your model, precision might be limited
            Assert.Equal(123.456m, dummy.DecimalProperty, 3);
        }
    }
}
