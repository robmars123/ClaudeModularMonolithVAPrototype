using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.EmailWorker;

/// <summary>
/// EmailWorker is a standalone process with no shared database of its own — a small local SQLite
/// file is enough to make it independently idempotent, without depending on the API host's SQL Server.
/// </summary>
public sealed class EmailWorkerDbContext(DbContextOptions<EmailWorkerDbContext> options) : DbContext(options), IHasInbox
{
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ConfigureInbox();
}
