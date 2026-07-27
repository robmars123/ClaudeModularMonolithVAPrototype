using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Features.GetInventoryByProduct;

public class GetInventoryByProductHandler : IRequestHandler<GetInventoryByProductQuery, Result<InventoryResponse>>
{
    private readonly InventoryDbContext _db;
    public GetInventoryByProductHandler(InventoryDbContext db) => _db = db;

    public async Task<Result<InventoryResponse>> Handle(GetInventoryByProductQuery q, CancellationToken ct)
    {
        InventoryItem? item = await _db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == q.ProductId, ct);
        if (item is null) return Result<InventoryResponse>.Failure("Inventory record not found.");
        return Result<InventoryResponse>.Success(new InventoryResponse(item.Id, item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.ReorderLevel, item.ReorderQuantity, item.NeedsReorder, item.CreatedAt, item.UpdatedAt));
    }
}
