namespace ECommerce.Modules.Products.Features.GetProducts;

public record ProductResponse(
    Guid Id, string Name, string Description, string SKU,
    decimal Price, string? ImageUrl, bool IsActive,
    Guid CategoryId, string CategoryName,
    DateTime CreatedAt, DateTime? UpdatedAt);
