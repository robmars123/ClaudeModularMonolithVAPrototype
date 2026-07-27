using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Features.GetInventory;

public class GetInventoryHandler : IRequestHandler<GetInventoryQuery, Result<PagedResult<InventoryResponse>>>
{
    private readonly InventoryDbContext _db;
    public GetInventoryHandler(InventoryDbContext db) => _db = db;

    public async Task<Result<PagedResult<InventoryResponse>>> Handle(GetInventoryQuery q, CancellationToken ct)
    {
        IQueryable<InventoryItem> query = _db.InventoryItems.AsNoTracking().AsQueryable();
        if (q.LowStockOnly) query = query.Where(i => i.QuantityOnHand - i.ReservedQuantity <= i.ReorderLevel);

        PagedResult<InventoryResponse> result = await query.OrderBy(i => i.ProductId).ToPagedResultAsync(
            q.Page, q.PageSize,
            i => new InventoryResponse(i.Id, i.ProductId, i.QuantityOnHand, i.ReservedQuantity, i.AvailableQuantity, i.ReorderLevel, i.ReorderQuantity, i.NeedsReorder, i.CreatedAt, i.UpdatedAt),
            ct);

        return Result<PagedResult<InventoryResponse>>.Success(result);
    }
}
