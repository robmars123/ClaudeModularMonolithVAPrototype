using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Inventory.Features.CreateInventoryItem;

public record CreateInventoryItemCommand(Guid ProductId, int InitialQuantity, int ReorderLevel, int ReorderQuantity, string CreatedBy)
    : IRequest<Result<InventoryResponse>>;
