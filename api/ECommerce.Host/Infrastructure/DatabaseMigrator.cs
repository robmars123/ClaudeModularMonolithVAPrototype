using ECommerce.Modules.Customers.Entities;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Host.Infrastructure;

public static class DatabaseMigrator
{
    public static async Task MigrateAndSeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying database migrations...");

            await services.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<ProductsDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<CustomersDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<OrdersDbContext>().Database.MigrateAsync();

            logger.LogInformation("Migrations applied successfully.");

            await SeedRolesAsync(services, logger);
            await SeedAdminUserAsync(services, logger, app.Configuration);
            await SeedSampleDataAsync(services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration/seeding.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["Admin", "Customer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider services, ILogger logger, IConfiguration config)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@ecommerce.com";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            FirstName = "System",
            LastName = "Admin",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var adminPassword = config["Seed:AdminPassword"] ?? "Admin@123456";
        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }
    }

    private static async Task SeedSampleDataAsync(IServiceProvider services, ILogger logger)
    {
        var productsContext = services.GetRequiredService<ProductsDbContext>();
        var inventoryContext = services.GetRequiredService<InventoryDbContext>();

        if (await productsContext.Categories.AnyAsync())
            return;

        logger.LogInformation("Seeding sample products and inventory...");

        var electronics = Category.Create("Electronics", "Electronic devices and accessories", "system");
        var clothing = Category.Create("Clothing", "Apparel and fashion", "system");
        var books = Category.Create("Books", "Books and publications", "system");

        productsContext.Categories.AddRange(electronics, clothing, books);
        await productsContext.SaveChangesAsync();

        var products = new[]
        {
            Product.Create("Wireless Headphones", "Premium noise-cancelling wireless headphones", "WH-001", 149.99m, electronics.Id, null, "system"),
            Product.Create("Mechanical Keyboard", "RGB mechanical gaming keyboard", "KB-001", 89.99m, electronics.Id, null, "system"),
            Product.Create("Classic T-Shirt", "100% cotton premium t-shirt", "TS-001", 29.99m, clothing.Id, null, "system"),
            Product.Create("Clean Code Book", "A handbook of agile software craftsmanship", "BK-001", 39.99m, books.Id, null, "system"),
        };

        productsContext.Products.AddRange(products);
        await productsContext.SaveChangesAsync();

        var inventoryItems = products.Select(p =>
            InventoryItem.Create(p.Id, 100, 10, 50, "system"));

        inventoryContext.InventoryItems.AddRange(inventoryItems);
        await inventoryContext.SaveChangesAsync();

        logger.LogInformation("Sample data seeded successfully.");
    }
}
