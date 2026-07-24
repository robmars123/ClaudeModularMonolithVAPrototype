using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Inventory.Features.AddStock;

public class Endpoint : Endpoint<AddStockRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/inventory/{productId}/add-stock");
        Roles("Admin");
    }

    public override async Task HandleAsync(AddStockRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new AddStockCommand(req.ProductId, req.Quantity, userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record AddStockRequest(Guid ProductId, int Quantity);
