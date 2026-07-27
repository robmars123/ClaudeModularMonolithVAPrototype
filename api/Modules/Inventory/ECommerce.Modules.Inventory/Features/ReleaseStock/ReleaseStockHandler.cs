using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Features.ReleaseStock;

/// <summary>Serves the cross-module <see cref="ReleaseStockRequest"/> contract — compensation for a failed order.</summary>
public class ReleaseStockHandler : IRequestHandler<ReleaseStockRequest, Result>
{
    private readonly InventoryDbContext _db;
    public ReleaseStockHandler(InventoryDbContext db) => _db = db;

    public async Task<Result> Handle(ReleaseStockRequest request, CancellationToken ct)
    {
        foreach (StockLine line in request.Lines)
        {
            InventoryItem? item = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.ProductId == line.ProductId, ct);

            item?.Release(line.Quantity);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
