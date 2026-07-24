using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.GetProductById;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductResponse>>;
