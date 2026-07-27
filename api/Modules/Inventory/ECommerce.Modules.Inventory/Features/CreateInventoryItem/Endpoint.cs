using System.Security.Claims;
using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;
using ByProductEndpoint = ECommerce.Modules.Inventory.Features.GetInventoryByProduct.Endpoint;

namespace ECommerce.Modules.Inventory.Features.CreateInventoryItem;

public class Endpoint : Endpoint<CreateInventoryItemRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/inventory");
        Roles("Admin", "ProductManager");
    }

    public override async Task HandleAsync(CreateInventoryItemRequest req, CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Result<InventoryResponse> result = await _mediator.Send(new CreateInventoryItemCommand(req.ProductId, req.InitialQuantity, req.ReorderLevel, req.ReorderQuantity, userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 400, cancellation: ct); return; }
        await Send.CreatedAtAsync<ByProductEndpoint>(new { productId = result.Value!.ProductId }, result.Value, cancellation: ct);
    }
}

public record CreateInventoryItemRequest(Guid ProductId, int InitialQuantity, int ReorderLevel, int ReorderQuantity);
