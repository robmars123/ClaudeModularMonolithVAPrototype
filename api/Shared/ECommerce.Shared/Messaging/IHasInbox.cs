using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Messaging;

/// <summary>Implemented by a module's own <see cref="DbContext"/> so it can deduplicate inbound broker messages.</summary>
public interface IHasInbox
{
    DbSet<InboxMessage> InboxMessages { get; }
}
