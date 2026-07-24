using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Features.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    private readonly ProductsDbContext _db;
    public GetProductByIdHandler(ProductsDbContext db) => _db = db;

    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery q, CancellationToken ct)
    {
        var p = await _db.Products.Include(x => x.Category).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == q.ProductId, ct);

        if (p is null) return Result<ProductResponse>.Failure("Product not found.");

        return Result<ProductResponse>.Success(new ProductResponse(p.Id, p.Name, p.Description, p.SKU, p.Price, p.ImageUrl, p.IsActive, p.CategoryId, p.Category.Name, p.CreatedAt, p.UpdatedAt));
    }
}
