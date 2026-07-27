using ECommerce.Host.Infrastructure;
using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ECommerce.Host.Tests;

public sealed class CorrelationIdMiddlewareTests
{
    private const string HeaderName = "X-Correlation-Id";

    [Fact]
    public async Task Generates_a_correlation_id_when_the_request_has_none()
    {
        CorrelationIdAccessor correlation = new CorrelationIdAccessor();
        // AsyncLocal changes only flow into code awaited from within InvokeAsync (i.e. the rest of
        // the pipeline via "next") — exactly like production — not back out to this test after it
        // returns, so the downstream assertion has to happen inside "next" itself.
        string? seenByDownstreamPipeline = null;
        CorrelationIdMiddleware middleware = new CorrelationIdMiddleware(_ =>
        {
            seenByDownstreamPipeline = correlation.CorrelationId;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, correlation, NullLogger<CorrelationIdMiddleware>.Instance);

        Assert.False(string.IsNullOrWhiteSpace(seenByDownstreamPipeline));
        Assert.Equal(seenByDownstreamPipeline, context.Response.Headers[HeaderName].ToString());
    }

    [Fact]
    public async Task Reuses_an_incoming_correlation_id_instead_of_generating_a_new_one()
    {
        CorrelationIdAccessor correlation = new CorrelationIdAccessor();
        string? seenByDownstreamPipeline = null;
        CorrelationIdMiddleware middleware = new CorrelationIdMiddleware(_ =>
        {
            seenByDownstreamPipeline = correlation.CorrelationId;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "caller-supplied-id";

        await middleware.InvokeAsync(context, correlation, NullLogger<CorrelationIdMiddleware>.Instance);

        Assert.Equal("caller-supplied-id", seenByDownstreamPipeline);
        Assert.Equal("caller-supplied-id", context.Response.Headers[HeaderName].ToString());
    }
}
