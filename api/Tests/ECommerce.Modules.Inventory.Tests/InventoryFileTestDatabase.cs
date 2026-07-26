using ECommerce.Modules.Inventory.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Tests;

/// <summary>
/// A real file-backed SQLite database (not the usual shared ":memory:" connection) so multiple
/// DbContext instances on different connections can genuinely run concurrently — needed to exercise
/// ReserveStockHandler's guarded ExecuteUpdate under real concurrent access instead of a single
/// serialized connection.
/// </summary>
public sealed class InventoryFileTestDatabase : IDisposable
{
    private readonly string _path;

    public InventoryFileTestDatabase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"inventory-tests-{Guid.NewGuid():N}.db");

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite($"Data Source={_path};Cache=Shared;Pooling=False")
            .Options;

        var context = new InventoryDbContext(options);
        // WAL + a busy timeout let concurrent writers retry instead of immediately throwing
        // "database is locked" — the point of this test double is genuine concurrent access.
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        context.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
        return context;
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var file = _path + suffix;
            try
            {
                if (File.Exists(file)) File.Delete(file);
            }
            catch (IOException)
            {
                // Best-effort cleanup of a temp file — a lingering OS-level handle must not fail the test.
            }
        }
    }
}
