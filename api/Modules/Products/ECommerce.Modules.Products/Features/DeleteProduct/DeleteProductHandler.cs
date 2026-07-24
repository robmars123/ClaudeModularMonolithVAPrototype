using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Products.Features.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly ProductsDbContext _db;
    public DeleteProductHandler(ProductsDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var product = await _db.Products.FindAsync([cmd.ProductId], ct);
        if (product is null) return Result.Failure("Product not found.");
        product.Deactivate(cmd.DeletedBy);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
