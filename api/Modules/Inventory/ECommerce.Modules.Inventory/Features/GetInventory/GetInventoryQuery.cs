using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Inventory.Features.GetInventory;

public record GetInventoryQuery(int Page = 1, int PageSize = 20, bool LowStockOnly = false)
    : IRequest<Result<PagedResult<InventoryResponse>>>;

public record InventoryResponse(
    Guid Id, Guid ProductId,
    int QuantityOnHand, int ReservedQuantity, int AvailableQuantity,
    int ReorderLevel, int ReorderQuantity, bool NeedsReorder,
    DateTime CreatedAt, DateTime? UpdatedAt);
