using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace ECommerce.Host.Infrastructure.HealthChecks;

public sealed class RabbitMqHealthCheck(IRabbitMqConnectionProvider connections) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        IChannel? channel = await connections.GetChannelAsync(cancellationToken);
        if (channel is null)
            return HealthCheckResult.Unhealthy("RabbitMQ is unreachable.");

        await using (channel.ConfigureAwait(false))
            return HealthCheckResult.Healthy();
    }
}
