using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.IntegrationEvents;

/// <summary>Published by Orders after an order is persisted. Inventory subscribes to commit the reserved stock.</summary>
public sealed record OrderPlacedEvent(Guid OrderId, string OrderNumber, string UserId, IReadOnlyList<StockLine> Lines) : INotification
{
    /// <summary>Schema version of this event's shape, so a consumer can branch on it if the shape ever changes.</summary>
    public int Version { get; init; } = 1;
}
