using ECommerce.Modules.Orders.Entities;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Persistence;

public class OrdersDbContext : DbContext, IHasOutbox
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");
        modelBuilder.ConfigureOutbox();

        modelBuilder.Entity<Cart>(e =>
        {
            e.ToTable("Carts");
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.Ignore(x => x.TotalAmount);
            e.HasIndex(x => x.UserId);
            e.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.ToTable("CartItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.Quantity).IsRequired();
            e.Ignore(x => x.TotalPrice);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
            e.Property(x => x.Status).IsRequired().HasConversion<int>();
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ShippingAddress).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.Quantity).IsRequired();
            e.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        });
    }
}
