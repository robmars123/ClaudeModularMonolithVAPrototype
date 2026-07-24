using ECommerce.Modules.Customers.Entities;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Persistence;

public class CustomersDbContext : DbContext, IHasInbox
{
    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.HasDefaultSchema("customers");
        m.ConfigureInbox();

        m.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.UserId).IsUnique();
            e.HasIndex(c => c.Email).IsUnique();
            e.Property(c => c.UserId).HasMaxLength(450).IsRequired();
            e.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            e.Property(c => c.Email).HasMaxLength(256).IsRequired();
            e.Property(c => c.Phone).HasMaxLength(20);
            e.OwnsOne(c => c.ShippingAddress, a =>
            {
                a.Property(x => x.Street).HasMaxLength(200);
                a.Property(x => x.City).HasMaxLength(100);
                a.Property(x => x.State).HasMaxLength(100);
                a.Property(x => x.ZipCode).HasMaxLength(20);
                a.Property(x => x.Country).HasMaxLength(100);
            });
        });
    }
}
