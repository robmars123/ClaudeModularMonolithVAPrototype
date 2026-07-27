using System.Collections.Concurrent;
using System.Reflection;
using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.Behaviors;

/// <summary>
/// Runs <see cref="IValidatableRequest.Validate"/> (when the request implements it) before the handler,
/// short-circuiting to the response type's own Failure(...) factory — no dependency on a validation
/// library, this only knows the Result/Result&lt;T&gt; shape every handler already returns.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ConcurrentDictionary<Type, Func<IReadOnlyList<string>, TResponse>> FailureFactories = new();

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IValidatableRequest validatable)
        {
            List<string> errors = validatable.Validate().Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            if (errors.Count > 0)
                return Task.FromResult(BuildFailure(errors));
        }

        return next();
    }

    private static TResponse BuildFailure(IReadOnlyList<string> errors)
    {
        Func<IReadOnlyList<string>, TResponse> factory = FailureFactories.GetOrAdd(typeof(TResponse), static responseType =>
        {
            MethodInfo? listFactory = responseType.GetMethod("Failure", [typeof(IReadOnlyList<string>)]);
            MethodInfo? stringFactory = responseType.GetMethod("Failure", [typeof(string)]);

            if (listFactory is not null)
                return errs => (TResponse)listFactory.Invoke(null, [errs])!;

            if (stringFactory is not null)
                return errs => (TResponse)stringFactory.Invoke(null, [string.Join(" ", errs)])!;

            throw new InvalidOperationException(
                $"'{responseType.FullName}' has no Failure(...) factory usable by ValidationBehavior. " +
                "Requests implementing IValidatableRequest must return Result or Result<T>.");
        });

        return factory(errors);
    }
}
