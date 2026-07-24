using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Products.Entities;

public class Category : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<Product> _products = [];
    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    private Category() { }

    public static Category Create(string name, string? description, string createdBy)
    {
        var c = new Category { Name = name, Description = description };
        c.SetCreatedBy(createdBy);
        return c;
    }

    public void Update(string name, string? description, string updatedBy)
    {
        Name = name; Description = description;
        SetUpdatedBy(updatedBy);
    }
}
