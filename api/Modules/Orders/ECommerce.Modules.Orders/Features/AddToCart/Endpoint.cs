using System.Security.Claims;
using ECommerce.Modules.Orders.Features.GetCart;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.AddToCart;

public class Endpoint : Endpoint<AddToCartRequest, CartResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Post("/api/cart/items");

    public override async Task HandleAsync(AddToCartRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new AddToCartCommand(userId, req.ProductId, req.Quantity), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 400, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record AddToCartRequest(Guid ProductId, int Quantity);
