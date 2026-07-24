using ECommerce.Modules.Products.Features.CreateCategory;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Features.GetCategories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>
{
    private readonly ProductsDbContext _db;
    public GetCategoriesHandler(ProductsDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(GetCategoriesQuery q, CancellationToken ct)
    {
        var categories = await _db.Categories.Where(c => c.IsActive).AsNoTracking()
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.IsActive))
            .ToListAsync(ct);
        return Result<IReadOnlyList<CategoryResponse>>.Success(categories);
    }
}
