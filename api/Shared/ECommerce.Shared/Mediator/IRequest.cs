namespace ECommerce.Shared.Mediator;

/// <summary>Marker for a command or query dispatched via <see cref="ISender.Send{TResponse}"/> to exactly one handler.</summary>
public interface IRequest<TResponse>;

/// <summary>Handles a single <typeparamref name="TRequest"/>. Exactly one handler must be registered per request type.</summary>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
