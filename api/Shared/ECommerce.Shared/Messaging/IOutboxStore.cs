namespace ECommerce.Shared.Messaging;

/// <summary>
/// Read/drain side of a module's outbox table. Writing is a separate concern — see
/// <see cref="IIntegrationEventPublisher"/> — because the write must happen on the module's own
/// <c>DbContext</c> change tracker (atomic with the business save), while draining is a
/// best-effort, retryable background job that runs against its own, later scope.
/// </summary>
public interface IOutboxStore
{
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default);

    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);
}
