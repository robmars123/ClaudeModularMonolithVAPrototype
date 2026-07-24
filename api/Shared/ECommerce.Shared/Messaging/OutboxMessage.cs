namespace ECommerce.Shared.Messaging;

/// <summary>
/// A pending (or dispatched) broker message, written transactionally alongside the module's own
/// data by <see cref="IIntegrationEventPublisher"/> so a message is never lost between "event
/// produced" and "event handed to RabbitMQ". <see cref="OutboxDispatcher"/> drains it.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Simple event type name (e.g. "OrderPlacedEvent") — the consumer already knows the shape for its queue.</summary>
    public required string Type { get; init; }

    public required MessageKind Kind { get; init; }

    /// <summary>Queue name for <see cref="MessageKind.PointToPoint"/>, routing key for <see cref="MessageKind.PubSub"/>.</summary>
    public required string Destination { get; init; }

    public required string Payload { get; init; }

    /// <summary>Schema version of <see cref="Payload"/>'s event type, so a consumer can branch on shape changes later.</summary>
    public int Version { get; init; } = 1;

    /// <summary>Propagated from <see cref="ECommerce.Shared.Observability.ICorrelationIdAccessor"/> so the whole flow (API request → broker → consumer) shares one id.</summary>
    public string? CorrelationId { get; init; }

    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime? ProcessedAtUtc { get; set; }

    public string? Error { get; set; }

    public int Attempts { get; set; }
}
