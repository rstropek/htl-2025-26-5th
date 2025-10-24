using AppServices;
using AppServices.Importer;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace ImporterTests;

public class DatabaseWriterTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task ClearAllAsync_RemovesAllDummies()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Dummies.AddRange(
                new Dummy { Name = "Test1", DecimalProperty = 10.5m },
                new Dummy { Name = "Test2", DecimalProperty = 20.75m }
            );
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new DummyImportDatabaseWriter(context);
            await writer.ClearAllAsync();
        }


        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            // Assert
            var count = await context.Dummies.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task WriteDummiesAsync_AddsDummiesToDatabase()
    {
        // Arrange
        var dummies = new List<Dummy>
        {
            new() { Name = "Test1", DecimalProperty = 10.5m },
            new() { Name = "Test2", DecimalProperty = 20.75m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new DummyImportDatabaseWriter(context);
            await writer.ClearAllAsync();
            await writer.WriteDummiesAsync(dummies);
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Dummies.CountAsync();
            Assert.Equal(2, count);
        }
    }

    [Fact]
    public async Task TransactionMethods_CommitSucceeds()
    {
        // Arrange
        var dummies = new[]
        { 
            new Dummy { Name = "Test", DecimalProperty = 10.5m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new DummyImportDatabaseWriter(context);
            await writer.ClearAllAsync();
            await writer.BeginTransactionAsync();
            await writer.WriteDummiesAsync(dummies);
            await writer.CommitTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Dummies.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task TransactionMethods_RollbackSucceeds()
    {
        // Arrange
        var dummies = new Dummy[]
        {
            new() { Name = "Test", DecimalProperty = 10.5m }
        };

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var writer = new DummyImportDatabaseWriter(context);
            await writer.ClearAllAsync();
            await writer.BeginTransactionAsync();
            await writer.WriteDummiesAsync(dummies);
            await writer.RollbackTransactionAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var count = await context.Dummies.CountAsync();
            Assert.Equal(0, count);
        }
    }
}
