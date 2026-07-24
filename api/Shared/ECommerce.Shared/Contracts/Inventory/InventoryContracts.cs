using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.Contracts.Inventory;

/// <summary>Public API of the Inventory module. Handled inside Inventory; callable from any module.</summary>
public sealed record GetStockAvailabilityRequest(Guid ProductId) : IRequest<Result<int>>;

/// <summary>Reserves all lines atomically — either every line is reserved or none are.</summary>
public sealed record ReserveStockRequest(IReadOnlyList<StockLine> Lines) : IRequest<Result>;

/// <summary>Releases previously reserved stock (compensation for a failed/abandoned order).</summary>
public sealed record ReleaseStockRequest(IReadOnlyList<StockLine> Lines) : IRequest<Result>;

public sealed record StockLine(Guid ProductId, int Quantity);
