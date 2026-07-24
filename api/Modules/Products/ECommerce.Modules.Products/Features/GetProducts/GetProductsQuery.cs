using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.GetProducts;

public record GetProductsQuery(int Page, int PageSize, string? Search, Guid? CategoryId, bool? IsActive)
    : IRequest<Result<PagedResult<ProductResponse>>>;
