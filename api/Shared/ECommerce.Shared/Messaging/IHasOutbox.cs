using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Messaging;

/// <summary>
/// Implemented by a module's own <see cref="DbContext"/> so its outbox table lives in the same
/// schema/database/transaction as its business tables — writing an <see cref="OutboxMessage"/> is
/// then just another entity added to the same change tracker as the aggregate, committed by the
/// same <c>SaveChangesAsync</c> call. That's what makes the outbox pattern actually atomic.
/// </summary>
public interface IHasOutbox
{
    DbSet<OutboxMessage> OutboxMessages { get; }
}
