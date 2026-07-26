using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.IntegrationEvents;

/// <summary>
/// Published by Orders after an order is persisted. Inventory subscribes (point-to-point) to commit the
/// reserved stock; EmailWorker subscribes (pub/sub) to send the order-confirmation notification.
/// </summary>
public sealed record OrderPlacedEvent(Guid OrderId, string OrderNumber, string UserId, IReadOnlyList<StockLine> Lines) : INotification
{
    /// <summary>Schema version of this event's shape, so a consumer can branch on it if the shape ever changes.</summary>
    public int Version { get; init; } = 1;

    /// <summary>Best-effort recipient details for notification consumers — absent if the contact lookup failed; Inventory's stock-commit consumer ignores these.</summary>
    public string? CustomerEmail { get; init; }
    public string? CustomerName { get; init; }
}
