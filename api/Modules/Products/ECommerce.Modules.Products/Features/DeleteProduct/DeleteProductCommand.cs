using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.DeleteProduct;

public record DeleteProductCommand(Guid ProductId, string DeletedBy) : IRequest<Result>;
