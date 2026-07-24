using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.UpdateProduct;

public record UpdateProductCommand(
    Guid ProductId, string Name, string Description,
    decimal Price, Guid CategoryId, string? ImageUrl,
    string UpdatedBy) : IRequest<Result<ProductResponse>>;
