namespace ECommerce.Shared.Messaging;

/// <summary>How an outbox message is delivered to RabbitMQ.</summary>
public enum MessageKind
{
    /// <summary>Default exchange, routed straight to one named queue — exactly one consumer processes each message.</summary>
    PointToPoint,

    /// <summary>Topic exchange, routed by key to every queue bound to it — every subscriber gets its own copy.</summary>
    PubSub
}
