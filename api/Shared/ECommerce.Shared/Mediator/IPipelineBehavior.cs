namespace ECommerce.Shared.Mediator;

/// <summary>Continuation that invokes the next behavior in the pipeline, or the handler itself.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Cross-cutting middleware around request handling (logging, validation, transactions).
/// Behaviors run in registration order; each must call <paramref name="next"/> to continue the pipeline.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
