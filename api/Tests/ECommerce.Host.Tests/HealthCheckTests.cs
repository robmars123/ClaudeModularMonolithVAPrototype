using ECommerce.Host.Infrastructure.HealthChecks;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using RabbitMQ.Client;
using Xunit;

namespace ECommerce.Host.Tests;

public sealed class HealthCheckTests
{
    [Fact]
    public async Task SqlDatabaseHealthCheck_is_unhealthy_when_no_connection_string_is_configured()
    {
        SqlDatabaseHealthCheck check = new SqlDatabaseHealthCheck(null);
        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task SqlDatabaseHealthCheck_is_unhealthy_when_the_server_is_unreachable()
    {
        // A syntactically valid connection string pointing at a host nothing is listening on.
        SqlDatabaseHealthCheck check = new SqlDatabaseHealthCheck("Server=127.0.0.1,1;Database=doesnotmatter;Connect Timeout=1;TrustServerCertificate=True");
        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task RabbitMqHealthCheck_is_unhealthy_when_no_channel_can_be_opened()
    {
        IRabbitMqConnectionProvider connections = Substitute.For<IRabbitMqConnectionProvider>();
        connections.GetChannelAsync(Arg.Any<CancellationToken>()).Returns((IChannel?)null);

        RabbitMqHealthCheck check = new RabbitMqHealthCheck(connections);
        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task RabbitMqHealthCheck_is_healthy_when_a_channel_can_be_opened()
    {
        IChannel channel = Substitute.For<IChannel>();
        IRabbitMqConnectionProvider connections = Substitute.For<IRabbitMqConnectionProvider>();
        connections.GetChannelAsync(Arg.Any<CancellationToken>()).Returns(channel);

        RabbitMqHealthCheck check = new RabbitMqHealthCheck(connections);
        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
}
