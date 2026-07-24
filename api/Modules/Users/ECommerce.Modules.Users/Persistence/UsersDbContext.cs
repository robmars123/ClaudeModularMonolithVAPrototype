using ECommerce.Modules.Users.Entities;
using ECommerce.Shared.Messaging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Users.Persistence;

public class UsersDbContext : IdentityDbContext<ApplicationUser>, IHasOutbox
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("users");
        builder.ConfigureOutbox();

        builder.Entity<ApplicationUser>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        });
    }
}
