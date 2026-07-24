using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
{
    private readonly ProductsDbContext _db;
    public CreateCategoryHandler(ProductsDbContext db) => _db = db;

    public async Task<Result<CategoryResponse>> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var category = Category.Create(cmd.Name, cmd.Description, cmd.CreatedBy);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return Result<CategoryResponse>.Success(new CategoryResponse(category.Id, category.Name, category.Description, category.IsActive));
    }
}
