using ECommerce.Shared.Observability;

namespace ECommerce.Host.Infrastructure;

/// <summary>
/// Reads (or generates) the correlation id for the request, stores it on <see cref="ICorrelationIdAccessor"/>
/// so it flows through the mediator pipeline, the outbox row, and any RabbitMQ message published
/// during the request, and echoes it back on the response for the caller to log against.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlation, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString("n");

        correlation.CorrelationId = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}
