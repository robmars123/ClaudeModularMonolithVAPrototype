using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Shared.Messaging;

public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionProvider> logger)
    : IRabbitMqConnectionProvider, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public async Task<IChannel?> GetChannelAsync(CancellationToken ct = default)
    {
        IConnection? connection = await GetConnectionAsync(ct).ConfigureAwait(false);
        if (connection is null)
            return null;

        try
        {
            return await connection.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not open a RabbitMQ channel — message stays in the outbox for retry.");
            return null;
        }
    }

    private async Task<IConnection?> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(ct).ConfigureAwait(false);
            return _connection;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not connect to RabbitMQ at {Host}:{Port} — falling back to no-op; the outbox will retry.", _options.HostName, _options.Port);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
