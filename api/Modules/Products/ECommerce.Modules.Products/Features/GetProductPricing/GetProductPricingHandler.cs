using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Products;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Features.GetProductPricing;

/// <summary>Serves the cross-module <see cref="GetProductPricingRequest"/> contract (used by Orders).</summary>
public class GetProductPricingHandler : IRequestHandler<GetProductPricingRequest, Result<ProductPricingDto>>
{
    private readonly ProductsDbContext _db;
    public GetProductPricingHandler(ProductsDbContext db) => _db = db;

    public async Task<Result<ProductPricingDto>> Handle(GetProductPricingRequest request, CancellationToken ct)
    {
        var p = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductId, ct);

        if (p is null) return Result<ProductPricingDto>.Failure("Product not found.");

        return Result<ProductPricingDto>.Success(new ProductPricingDto(p.Id, p.Name, p.SKU, p.Price, p.IsActive));
    }
}
