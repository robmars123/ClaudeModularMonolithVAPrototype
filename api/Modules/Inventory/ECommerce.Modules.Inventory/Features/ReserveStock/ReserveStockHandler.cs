using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Modules.Inventory.Features.ReserveStock;

/// <summary>
/// Serves the cross-module <see cref="ReserveStockRequest"/> contract.
/// All lines are reserved atomically: conditional set-based updates inside one transaction,
/// so concurrent orders can never oversell a product.
/// </summary>
public class ReserveStockHandler : IRequestHandler<ReserveStockRequest, Result>
{
    private readonly InventoryDbContext _db;
    public ReserveStockHandler(InventoryDbContext db) => _db = db;

    public async Task<Result> Handle(ReserveStockRequest request, CancellationToken ct)
    {
        if (request.Lines.Count == 0)
            return Result.Failure("Nothing to reserve.");

        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(ct);

        foreach (StockLine line in request.Lines)
        {
            // Guarded update: only reserves when enough unreserved stock remains at commit time.
            int updated = await _db.InventoryItems
                .Where(i => i.ProductId == line.ProductId
                            && i.QuantityOnHand - i.ReservedQuantity >= line.Quantity)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.ReservedQuantity, i => i.ReservedQuantity + line.Quantity)
                    .SetProperty(i => i.UpdatedAt, DateTime.UtcNow), ct);

            if (updated == 0)
            {
                await transaction.RollbackAsync(ct);
                return Result.Failure($"Insufficient stock for product '{line.ProductId}'.");
            }
        }

        await transaction.CommitAsync(ct);
        return Result.Success();
    }
}
