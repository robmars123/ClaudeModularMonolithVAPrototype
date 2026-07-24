using ECommerce.Modules.Orders.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Tests;

/// <summary>Relational, transaction-capable SQLite database (kept alive via an open ":memory:" connection) for one test.</summary>
public sealed class OrdersSqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public OrdersSqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public OrdersDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>().UseSqlite(_connection).Options;
        return new OrdersDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
