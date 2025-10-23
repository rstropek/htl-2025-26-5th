using AppServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace TestInfrastructure;

public class DatabaseFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public DbContextOptions<ApplicationDataContext> Options { get; }

    public DatabaseFixture()
    {
        // Create and open a connection. This creates the SQLite in-memory database
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        Options = new DbContextOptionsBuilder<ApplicationDataContext>()
            .UseSqlite(Connection)
            .Options;

        // Create the schema in the database
        using var context = new ApplicationDataContext(Options);
        context.Database.EnsureCreated();
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        Connection.Dispose();
    }
}
