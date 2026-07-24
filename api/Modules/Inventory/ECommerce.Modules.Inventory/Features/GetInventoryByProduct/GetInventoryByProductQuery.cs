using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Inventory.Features.GetInventoryByProduct;

public record GetInventoryByProductQuery(Guid ProductId) : IRequest<Result<InventoryResponse>>;
