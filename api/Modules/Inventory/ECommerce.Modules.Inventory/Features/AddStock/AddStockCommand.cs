using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Inventory.Features.AddStock;

public record AddStockCommand(Guid ProductId, int Quantity, string UserId) : IRequest<Result<InventoryResponse>>;
