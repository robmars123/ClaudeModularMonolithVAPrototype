using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Features.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductResponse>>>
{
    private readonly ProductsDbContext _db;
    public GetProductsHandler(ProductsDbContext db) => _db = db;

    public async Task<Result<PagedResult<ProductResponse>>> Handle(GetProductsQuery q, CancellationToken ct)
    {
        var query = _db.Products.Include(p => p.Category).AsNoTracking().AsQueryable();

        if (q.IsActive.HasValue) query = query.Where(p => p.IsActive == q.IsActive.Value);
        if (q.CategoryId.HasValue) query = query.Where(p => p.CategoryId == q.CategoryId.Value);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.SKU.ToLower().Contains(s));
        }

        var result = await query.OrderBy(p => p.Name).ToPagedResultAsync(
            q.Page, q.PageSize,
            p => new ProductResponse(p.Id, p.Name, p.Description, p.SKU, p.Price, p.ImageUrl, p.IsActive, p.CategoryId, p.Category.Name, p.CreatedAt, p.UpdatedAt),
            ct);

        return Result<PagedResult<ProductResponse>>.Success(result);
    }
}
