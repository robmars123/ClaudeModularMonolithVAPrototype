using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Products.Entities;

public class Product : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    private Product() { }

    public static Product Create(string name, string description, string sku, decimal price, Guid categoryId, string? imageUrl, string createdBy)
    {
        Product p = new Product { Name = name, Description = description, SKU = sku, Price = price, CategoryId = categoryId, ImageUrl = imageUrl };
        p.SetCreatedBy(createdBy);
        return p;
    }

    public void Update(string name, string description, decimal price, Guid categoryId, string? imageUrl, string updatedBy)
    {
        Name = name; Description = description; Price = price; CategoryId = categoryId; ImageUrl = imageUrl;
        SetUpdatedBy(updatedBy);
    }

    public void Deactivate(string updatedBy) { IsActive = false; SetUpdatedBy(updatedBy); }
}
