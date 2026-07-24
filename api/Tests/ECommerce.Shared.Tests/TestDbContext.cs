using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Tests;

public sealed class TestAggregate
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

/// <summary>Minimal DbContext with one business table + the outbox/inbox tables, for exercising atomicity without any module's real schema.</summary>
public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IHasOutbox, IHasInbox
{
    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureOutbox();
        modelBuilder.ConfigureInbox();

        modelBuilder.Entity<TestAggregate>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).IsRequired();
            e.HasIndex(a => a.Name).IsUnique();
        });
    }
}
