using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Features.ReserveStock;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Inventory;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.Modules.Inventory.Tests;

public sealed class ReserveStockHandlerTests
{
    [Fact]
    public async Task Reserves_when_enough_stock_is_available()
    {
        using InventoryFileTestDatabase db = new InventoryFileTestDatabase();
        Guid productId = Guid.NewGuid();

        await using (InventoryDbContext setup = db.CreateContext())
        {
            setup.InventoryItems.Add(InventoryItem.Create(productId, 10, 2, 5, "system"));
            await setup.SaveChangesAsync();
        }

        await using InventoryDbContext context = db.CreateContext();
        ReserveStockHandler handler = new ReserveStockHandler(context);

        Result result = await handler.Handle(new ReserveStockRequest([new StockLine(productId, 4)]), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using InventoryDbContext verify = db.CreateContext();
        InventoryItem item = await verify.InventoryItems.SingleAsync(i => i.ProductId == productId);
        Assert.Equal(4, item.ReservedQuantity);
    }

    [Fact]
    public async Task Fails_and_reserves_nothing_when_stock_is_insufficient()
    {
        using InventoryFileTestDatabase db = new InventoryFileTestDatabase();
        Guid productId = Guid.NewGuid();

        await using (InventoryDbContext setup = db.CreateContext())
        {
            setup.InventoryItems.Add(InventoryItem.Create(productId, 3, 1, 5, "system"));
            await setup.SaveChangesAsync();
        }

        await using InventoryDbContext context = db.CreateContext();
        ReserveStockHandler handler = new ReserveStockHandler(context);

        Result result = await handler.Handle(new ReserveStockRequest([new StockLine(productId, 10)]), CancellationToken.None);

        Assert.False(result.IsSuccess);

        await using InventoryDbContext verify = db.CreateContext();
        InventoryItem item = await verify.InventoryItems.SingleAsync(i => i.ProductId == productId);
        Assert.Equal(0, item.ReservedQuantity);
    }

    [Fact]
    public async Task Concurrent_reservations_never_oversell()
    {
        // Only enough stock for one of two concurrent 10-unit checkouts.
        using InventoryFileTestDatabase db = new InventoryFileTestDatabase();
        Guid productId = Guid.NewGuid();

        await using (InventoryDbContext setup = db.CreateContext())
        {
            setup.InventoryItems.Add(InventoryItem.Create(productId, 10, 2, 5, "system"));
            await setup.SaveChangesAsync();
        }

        Task<bool> ReserveAsync() => Task.Run(async () =>
        {
            await using InventoryDbContext context = db.CreateContext();
            ReserveStockHandler handler = new ReserveStockHandler(context);
            Result result = await handler.Handle(new ReserveStockRequest([new StockLine(productId, 10)]), CancellationToken.None);
            return result.IsSuccess;
        });

        bool[] outcomes = await Task.WhenAll(ReserveAsync(), ReserveAsync());

        Assert.Single(outcomes, success => success);
        Assert.Single(outcomes, success => !success);

        await using InventoryDbContext verify = db.CreateContext();
        InventoryItem item = await verify.InventoryItems.SingleAsync(i => i.ProductId == productId);
        Assert.Equal(10, item.ReservedQuantity); // never exceeds QuantityOnHand — no oversell
    }
}
