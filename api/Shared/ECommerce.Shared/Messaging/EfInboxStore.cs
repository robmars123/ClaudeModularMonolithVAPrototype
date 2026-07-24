using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Messaging;

/// <summary>Generic over the consuming module's own <typeparamref name="TDbContext"/>.</summary>
public sealed class EfInboxStore<TDbContext>(TDbContext db) : IInboxStore
    where TDbContext : DbContext, IHasInbox
{
    public Task<bool> IsProcessedAsync(string messageId, string consumer, CancellationToken ct = default) =>
        db.InboxMessages.AnyAsync(i => i.MessageId == messageId && i.Consumer == consumer, ct);

    public async Task MarkProcessedAsync(string messageId, string consumer, CancellationToken ct = default)
    {
        db.InboxMessages.Add(new InboxMessage { MessageId = messageId, Consumer = consumer });
        await db.SaveChangesAsync(ct);
    }
}
