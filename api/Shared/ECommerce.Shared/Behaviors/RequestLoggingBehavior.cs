using System.Diagnostics;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.Logging;

namespace ECommerce.Shared.Behaviors;

/// <summary>Logs every mediator request with its duration; failures are logged and rethrown.</summary>
public sealed class RequestLoggingBehavior<TRequest, TResponse>(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger, ICorrelationIdAccessor correlation)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Stopwatch stopwatch = Stopwatch.StartNew();

        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = correlation.CorrelationId });

        try
        {
            TResponse? response = await next().ConfigureAwait(false);
            logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in {RequestName} after {ElapsedMs} ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
