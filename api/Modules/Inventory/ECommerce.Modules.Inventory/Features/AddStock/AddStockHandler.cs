using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Features.AddStock;

public class AddStockHandler : IRequestHandler<AddStockCommand, Result<InventoryResponse>>
{
    private readonly InventoryDbContext _db;
    public AddStockHandler(InventoryDbContext db) => _db = db;

    public async Task<Result<InventoryResponse>> Handle(AddStockCommand cmd, CancellationToken ct)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == cmd.ProductId, ct);
        if (item is null) return Result<InventoryResponse>.Failure("Inventory record not found.");
        item.AddStock(cmd.Quantity, cmd.UserId);
        await _db.SaveChangesAsync(ct);
        return Result<InventoryResponse>.Success(new InventoryResponse(item.Id, item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.ReorderLevel, item.ReorderQuantity, item.NeedsReorder, item.CreatedAt, item.UpdatedAt));
    }
}
