using ECommerce.Modules.Products.Features.CreateCategory;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.GetCategories;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryResponse>>>;
