using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Features.CreateInventoryItem;

public class CreateInventoryItemHandler : IRequestHandler<CreateInventoryItemCommand, Result<InventoryResponse>>
{
    private readonly InventoryDbContext _db;
    public CreateInventoryItemHandler(InventoryDbContext db) => _db = db;

    public async Task<Result<InventoryResponse>> Handle(CreateInventoryItemCommand cmd, CancellationToken ct)
    {
        if (await _db.InventoryItems.AnyAsync(i => i.ProductId == cmd.ProductId, ct))
            return Result<InventoryResponse>.Failure("Inventory record already exists.");

        InventoryItem item = InventoryItem.Create(cmd.ProductId, cmd.InitialQuantity, cmd.ReorderLevel, cmd.ReorderQuantity, cmd.CreatedBy);
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return Result<InventoryResponse>.Success(new InventoryResponse(item.Id, item.ProductId, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity, item.ReorderLevel, item.ReorderQuantity, item.NeedsReorder, item.CreatedAt, item.UpdatedAt));
    }
}
