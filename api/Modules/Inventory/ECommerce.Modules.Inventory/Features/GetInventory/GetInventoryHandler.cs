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
        var query = _db.InventoryItems.AsNoTracking().AsQueryable();
        if (q.LowStockOnly) query = query.Where(i => i.QuantityOnHand - i.ReservedQuantity <= i.ReorderLevel);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(i => i.ProductId).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(i => new InventoryResponse(i.Id, i.ProductId, i.QuantityOnHand, i.ReservedQuantity, i.AvailableQuantity, i.ReorderLevel, i.ReorderQuantity, i.NeedsReorder, i.CreatedAt, i.UpdatedAt))
            .ToListAsync(ct);

        return Result<PagedResult<InventoryResponse>>.Success(new PagedResult<InventoryResponse>(items, total, q.Page, q.PageSize));
    }
}
