using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.CreateProduct;

public record CreateProductCommand(
    string Name, string Description, string SKU,
    decimal Price, Guid CategoryId, string? ImageUrl,
    string CreatedBy) : IRequest<Result<ProductResponse>>;
