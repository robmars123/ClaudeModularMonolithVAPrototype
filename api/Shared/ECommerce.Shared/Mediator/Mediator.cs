using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shared.Mediator;

/// <summary>
/// Reflection-free-on-the-hot-path mediator. The concrete request/notification type is resolved once
/// per closed generic type into a cached dispatcher; every subsequent dispatch is a virtual call.
/// Registered scoped so handlers resolve from the current request scope (DbContexts, etc.).
/// </summary>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, object> RequestDispatchers = new();
    private static readonly ConcurrentDictionary<Type, NotificationDispatcher> NotificationDispatchers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dispatcher = (RequestDispatcher<TResponse>)RequestDispatchers.GetOrAdd(
            request.GetType(),
            static requestType => Activator.CreateInstance(
                typeof(RequestDispatcher<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return dispatcher.Dispatch(request, serviceProvider, cancellationToken);
    }

    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var dispatcher = NotificationDispatchers.GetOrAdd(
            notification.GetType(),
            static notificationType => (NotificationDispatcher)Activator.CreateInstance(
                typeof(NotificationDispatcher<>).MakeGenericType(notificationType))!);

        return dispatcher.Dispatch(notification, serviceProvider, cancellationToken);
    }

    private abstract class RequestDispatcher<TResponse>
    {
        public abstract Task<TResponse> Dispatch(IRequest<TResponse> request, IServiceProvider services, CancellationToken cancellationToken);
    }

    private sealed class RequestDispatcher<TRequest, TResponse> : RequestDispatcher<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Dispatch(IRequest<TResponse> request, IServiceProvider services, CancellationToken cancellationToken)
        {
            var handler = services.GetService<IRequestHandler<TRequest, TResponse>>()
                ?? throw new InvalidOperationException(
                    $"No handler registered for '{typeof(TRequest).FullName}'. " +
                    "Ensure the owning module passes its assembly to AddMediator().");

            RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle((TRequest)request, cancellationToken);

            // Wrap behaviors in reverse so the first registered behavior is outermost.
            foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
            {
                var next = pipeline;
                pipeline = () => behavior.Handle((TRequest)request, next, cancellationToken);
            }

            return pipeline();
        }
    }

    private abstract class NotificationDispatcher
    {
        public abstract Task Dispatch(INotification notification, IServiceProvider services, CancellationToken cancellationToken);
    }

    private sealed class NotificationDispatcher<TNotification> : NotificationDispatcher
        where TNotification : INotification
    {
        public override async Task Dispatch(INotification notification, IServiceProvider services, CancellationToken cancellationToken)
        {
            List<Exception>? failures = null;

            // Run every handler even if an earlier one fails, then surface all failures together —
            // one module's subscriber must not silently starve another's.
            foreach (var handler in services.GetServices<INotificationHandler<TNotification>>())
            {
                try
                {
                    await handler.Handle((TNotification)notification, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    (failures ??= []).Add(ex);
                }
            }

            if (failures is not null)
            {
                throw failures.Count == 1
                    ? failures[0]
                    : new AggregateException($"{failures.Count} handler(s) failed for '{typeof(TNotification).Name}'.", failures);
            }
        }
    }
}
