using System.Text.Json;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Observability;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Messaging;

/// <summary>Generic over the calling module's own <typeparamref name="TDbContext"/> so every module gets this for free.</summary>
public sealed class EfOutboxIntegrationEventPublisher<TDbContext>(TDbContext db, ICorrelationIdAccessor correlation) : IIntegrationEventPublisher
    where TDbContext : DbContext, IHasOutbox
{
    public void Enqueue<TEvent>(TEvent @event, MessageKind kind, string destination)
        where TEvent : INotification
    {
        db.OutboxMessages.Add(new OutboxMessage
        {
            Type = typeof(TEvent).Name,
            Kind = kind,
            Destination = destination,
            Payload = JsonSerializer.Serialize(@event),
            CorrelationId = correlation.CorrelationId
        });
    }
}
