using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.Messaging;

/// <summary>
/// Entry point modules call, right where an integration event is produced from the API (alongside
/// the existing in-process <see cref="IPublisher.Publish"/>), to also hand the event to RabbitMQ.
/// <see cref="Enqueue{TEvent}"/> only adds the outbox row to the module's own <c>DbContext</c>
/// change tracker — it does not call <c>SaveChangesAsync</c> itself. Callers must enqueue before
/// their own <c>SaveChangesAsync</c> so the outbox row commits atomically with the business write;
/// <see cref="OutboxDispatcher"/> then delivers it asynchronously, so the API request is never
/// blocked on the broker.
/// </summary>
public interface IIntegrationEventPublisher
{
    void Enqueue<TEvent>(TEvent @event, MessageKind kind, string destination)
        where TEvent : INotification;
}
