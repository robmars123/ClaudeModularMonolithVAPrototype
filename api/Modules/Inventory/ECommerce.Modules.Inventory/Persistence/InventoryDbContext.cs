using ECommerce.Modules.Inventory.Entities;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Persistence;

public class InventoryDbContext : DbContext, IHasInbox
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.HasDefaultSchema("inventory");
        m.ConfigureInbox();
        m.Entity<InventoryItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.ProductId).IsUnique();
            e.Property(i => i.ProductId).IsRequired();
            e.Property(i => i.QuantityOnHand).IsRequired();
            e.Property(i => i.ReservedQuantity).IsRequired();
            e.Property(i => i.ReorderLevel).IsRequired();
            e.Property(i => i.ReorderQuantity).IsRequired();
        });
    }
}
