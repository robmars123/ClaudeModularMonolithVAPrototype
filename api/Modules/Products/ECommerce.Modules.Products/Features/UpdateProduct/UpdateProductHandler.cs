using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Products.Features.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<ProductResponse>>
{
    private readonly ProductsDbContext _db;
    public UpdateProductHandler(ProductsDbContext db) => _db = db;

    public async Task<Result<ProductResponse>> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        Product? product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == cmd.ProductId, ct);
        if (product is null) return Result<ProductResponse>.Failure("Product not found.");

        if (!await _db.Categories.AnyAsync(c => c.Id == cmd.CategoryId && c.IsActive, ct))
            return Result<ProductResponse>.Failure("Category not found.");

        product.Update(cmd.Name, cmd.Description, cmd.Price, cmd.CategoryId, cmd.ImageUrl, cmd.UpdatedBy);
        await _db.SaveChangesAsync(ct);

        Category? cat = await _db.Categories.FindAsync([cmd.CategoryId], ct);
        return Result<ProductResponse>.Success(new ProductResponse(product.Id, product.Name, product.Description, product.SKU, product.Price, product.ImageUrl, product.IsActive, product.CategoryId, cat!.Name, product.CreatedAt, product.UpdatedAt));
    }
}
