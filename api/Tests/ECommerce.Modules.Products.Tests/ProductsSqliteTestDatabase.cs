using ECommerce.Modules.Products.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Tests;

public sealed class ProductsSqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public ProductsSqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public ProductsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProductsDbContext>().UseSqlite(_connection).Options;
        return new ProductsDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
