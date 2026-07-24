using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Features.GetStockAvailability;

/// <summary>Serves the cross-module <see cref="GetStockAvailabilityRequest"/> contract. A product without an inventory record has 0 available.</summary>
public class GetStockAvailabilityHandler : IRequestHandler<GetStockAvailabilityRequest, Result<int>>
{
    private readonly InventoryDbContext _db;
    public GetStockAvailabilityHandler(InventoryDbContext db) => _db = db;

    public async Task<Result<int>> Handle(GetStockAvailabilityRequest request, CancellationToken ct)
    {
        var item = await _db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, ct);

        return Result<int>.Success(item?.AvailableQuantity ?? 0);
    }
}
