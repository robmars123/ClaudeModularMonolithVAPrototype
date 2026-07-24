using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.IntegrationEvents;

/// <summary>Published by Products after a product is created. Inventory subscribes to create the stock record.</summary>
public sealed record ProductCreatedEvent(Guid ProductId, string Sku, string Name, string CreatedBy) : INotification
{
    /// <summary>Schema version of this event's shape, so a consumer can branch on it if the shape ever changes.</summary>
    public int Version { get; init; } = 1;
}
