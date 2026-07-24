using ECommerce.Modules.Inventory.Features.GetInventory;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Inventory.Features.GetInventoryByProduct;

public class Endpoint : Endpoint<GetInventoryByProductRequest, InventoryResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/inventory/{productId}");
        Roles("Admin");
    }

    public override async Task HandleAsync(GetInventoryByProductRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInventoryByProductQuery(req.ProductId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record GetInventoryByProductRequest(Guid ProductId);
