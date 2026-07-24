using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Messaging;

public static class OutboxModelBuilderExtensions
{
    /// <summary>Maps <see cref="OutboxMessage"/> into the calling module's own (already-set) default schema.</summary>
    public static void ConfigureOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Type).IsRequired().HasMaxLength(256);
            e.Property(o => o.Kind).IsRequired().HasConversion<string>().HasMaxLength(32);
            e.Property(o => o.Destination).IsRequired().HasMaxLength(256);
            e.Property(o => o.Payload).IsRequired();
            e.Property(o => o.CorrelationId).HasMaxLength(64);
            e.Property(o => o.Error).HasMaxLength(2000);
            e.HasIndex(o => o.ProcessedAtUtc);
        });
    }

    /// <summary>Maps <see cref="InboxMessage"/> into the calling module's own (already-set) default schema.</summary>
    public static void ConfigureInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>(e =>
        {
            e.HasKey(i => new { i.MessageId, i.Consumer });
            e.Property(i => i.MessageId).IsRequired().HasMaxLength(64);
            e.Property(i => i.Consumer).IsRequired().HasMaxLength(256);
        });
    }
}
