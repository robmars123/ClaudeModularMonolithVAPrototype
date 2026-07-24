using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.CreateCategory;

public record CreateCategoryCommand(string Name, string? Description, string CreatedBy)
    : IRequest<Result<CategoryResponse>>;

public record CategoryResponse(Guid Id, string Name, string? Description, bool IsActive);
