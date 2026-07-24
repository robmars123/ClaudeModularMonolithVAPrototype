using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Tests;

/// <summary>
/// A relational, transaction-capable SQLite database kept alive for the lifetime of one test via an
/// open connection to ":memory:" — unlike the EF Core InMemory provider, this actually enforces
/// unique constraints and rolls back a failed <c>SaveChangesAsync</c>, which is exactly what the
/// outbox-atomicity tests need to exercise.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new TestDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
