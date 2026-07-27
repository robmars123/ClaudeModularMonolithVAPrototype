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
        // The caller (typically the HTTP client) walked away before the request finished — e.g. a
        // React effect cleanup aborting an in-flight fetch. Expected under normal use, not a bug, so
        // it's logged quietly instead of at Error level to keep that level meaningful.
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("{RequestName} cancelled by caller after {ElapsedMs} ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in {RequestName} after {ElapsedMs} ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
