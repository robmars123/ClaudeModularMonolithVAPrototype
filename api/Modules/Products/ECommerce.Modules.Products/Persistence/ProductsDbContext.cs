using ECommerce.Modules.Products.Entities;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Persistence;

public class ProductsDbContext : DbContext, IHasOutbox
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.HasDefaultSchema("products");
        m.ConfigureOutbox();

        m.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Description).HasMaxLength(2000).IsRequired();
            e.Property(p => p.SKU).HasMaxLength(50).IsRequired();
            e.HasIndex(p => p.SKU).IsUnique();
            e.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            e.Property(p => p.ImageUrl).HasMaxLength(500);
            e.HasOne(p => p.Category).WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
        });
    }
}
