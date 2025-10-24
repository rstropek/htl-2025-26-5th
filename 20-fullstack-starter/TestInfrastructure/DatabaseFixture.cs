using AppServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace TestInfrastructure;

/*
 * xUnit Fixtures - Overview
 * 
 * Fixtures in xUnit are used to create and share test context across multiple tests.
 * They help manage setup and teardown of expensive or shared resources efficiently.
 * 
 * Key Benefits:
 * - Resource Sharing: Create expensive resources once and reuse them across tests
 * - Performance: Avoid redundant setup/teardown operations for each test
 * - Clean Code: Separate test infrastructure from test logic
 * - Lifecycle Control: Explicit control over when resources are created and disposed
 * 
 * xUnit Fixture Types:
 * 
 * 1. Class Fixtures (IClassFixture<T>):
 *    - Created once before any test in the class runs
 *    - Shared across all tests in a single test class
 *    - Disposed after all tests in the class complete
 *    - Usage: Test class implements IClassFixture<YourFixture>
 * 
 * 2. Collection Fixtures (ICollectionFixture<T>):
 *    - Shared across multiple test classes
 *    - Created once before any test in the collection runs
 *    - Disposed after all tests in all classes of the collection complete
 *    - Usage: Define [CollectionDefinition] and apply [Collection] attribute to test classes
 * 
 * 3. Test Output Helper (ITestOutputHelper):
 *    - Not a fixture but a helper for test-specific logging
 *    - Automatically injected by xUnit into test constructors
 * 
 * Best Practices:
 * - Implement IDisposable to clean up resources
 * - Keep fixtures focused on a single responsibility
 * - Use class fixtures for per-class resources (like database connections)
 * - Use collection fixtures for resources shared across multiple test classes
 * - Avoid mutable state that could cause test interdependencies
 * - Make fixture initialization idempotent when possible
 */

/// <summary>
/// Database fixture for integration tests using SQLite in-memory database.
/// This fixture provides a shared database context configuration for tests that need
/// to interact with a database without affecting a real database instance.
/// </summary>
/// <remarks>
/// The fixture creates an in-memory SQLite database that exists for the lifetime of the fixture.
/// The connection must remain open for the in-memory database to persist. When the fixture
/// is disposed, the database is automatically deleted.
/// 
/// This is particularly useful for:
/// - Integration tests that need a real database schema
/// - Testing Entity Framework Core migrations and queries
/// - Avoiding test pollution by using an isolated database per test class
/// - Fast test execution compared to traditional database setup/teardown
/// </remarks>
public class DatabaseFixture : IDisposable
{
    /// <summary>
    /// Gets the SQLite connection to the in-memory database.
    /// This connection must remain open for the duration of the tests to keep the in-memory database alive.
    /// </summary>
    public SqliteConnection Connection { get; }
    
    /// <summary>
    /// Gets the Entity Framework Core database context options configured for the in-memory SQLite database.
    /// Use these options to create ApplicationDataContext instances in your tests.
    /// </summary>
    public DbContextOptions<ApplicationDataContext> Options { get; }

    /// <summary>
    /// Initializes a new instance of the DatabaseFixture class.
    /// Creates an in-memory SQLite database and applies the Entity Framework Core schema.
    /// </summary>
    /// <remarks>
    /// The initialization process:
    /// 1. Creates a SQLite connection with the special "DataSource=:memory:" connection string
    /// 2. Opens the connection (required to keep the in-memory database alive)
    /// 3. Configures Entity Framework Core to use this SQLite connection
    /// 4. Creates the database schema using EF Core's EnsureCreated() method
    /// 
    /// Note: The connection remains open until Dispose() is called. Closing the connection
    /// would destroy the in-memory database immediately.
    /// </remarks>
    public DatabaseFixture()
    {
        // Create and open a connection. This creates the SQLite in-memory database.
        // The DataSource=:memory: connection string tells SQLite to create an in-memory database
        // that exists only for the duration of this connection.
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        // Configure Entity Framework Core to use the SQLite connection.
        // These options will be used to create ApplicationDataContext instances in tests.
        Options = new DbContextOptionsBuilder<ApplicationDataContext>()
            .UseSqlite(Connection)
            .Options;

        // Create the database schema in the in-memory database.
        // EnsureCreated() creates all tables, indexes, and constraints defined in the model.
        // Note: This doesn't apply migrations; it creates the schema based on the current model.
        using var context = new ApplicationDataContext(Options);
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Releases all resources used by the DatabaseFixture.
    /// Closes the SQLite connection, which automatically destroys the in-memory database.
    /// </summary>
    /// <remarks>
    /// This method is called by xUnit after all tests using this fixture have completed.
    /// The CA1816 warning is suppressed because this class has no unmanaged resources
    /// and doesn't require a finalizer.
    /// </remarks>
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        // Disposing the connection closes it and destroys the in-memory database.
        // All data and schema are lost at this point, ensuring test isolation.
        Connection.Dispose();
    }
}
