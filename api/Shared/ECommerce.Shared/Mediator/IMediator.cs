namespace ECommerce.Shared.Mediator;

/// <summary>Dispatches a request to its single registered handler, through any registered pipeline behaviors.</summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>Publishes a notification to all registered handlers. All handlers run even if one fails.</summary>
public interface IPublisher
{
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}

/// <summary>In-process mediator: the only communication channel between modules.</summary>
public interface IMediator : ISender, IPublisher;
