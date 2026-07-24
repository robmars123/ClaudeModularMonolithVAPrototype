using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Messaging;

/// <summary>Generic over the calling module's own <typeparamref name="TDbContext"/> — reads/marks that module's outbox table.</summary>
public sealed class EfOutboxStore<TDbContext>(TDbContext db) : IOutboxStore
    where TDbContext : DbContext, IHasOutbox
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default) =>
        await db.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .OrderBy(m => m.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await db.OutboxMessages.FindAsync([id], ct);
        if (message is null)
            return;

        message.ProcessedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        var message = await db.OutboxMessages.FindAsync([id], ct);
        if (message is null)
            return;

        message.Attempts++;
        message.Error = error;
        await db.SaveChangesAsync(ct);
    }
}
