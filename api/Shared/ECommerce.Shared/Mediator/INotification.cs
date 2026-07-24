namespace ECommerce.Shared.Mediator;

/// <summary>Marker for an event published via <see cref="IPublisher.Publish"/> to zero or more handlers.</summary>
public interface INotification;

/// <summary>Handles a published <typeparamref name="TNotification"/>. Any number of handlers may be registered.</summary>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
