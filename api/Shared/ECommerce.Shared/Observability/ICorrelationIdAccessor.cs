namespace ECommerce.Shared.Observability;

/// <summary>
/// Flows one correlation id through a request end-to-end — HTTP request → mediator handler →
/// outbox row → RabbitMQ message → consumer — without threading it through every method
/// signature. Backed by <see cref="AsyncLocal{T}"/> so it works both under ASP.NET Core request
/// scopes and inside background-service message handlers, which have no <c>HttpContext</c>.
/// </summary>
public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; set; }
}
