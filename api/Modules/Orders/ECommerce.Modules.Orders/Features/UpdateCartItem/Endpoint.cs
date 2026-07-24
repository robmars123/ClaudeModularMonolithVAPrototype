using System.Security.Claims;
using ECommerce.Modules.Orders.Features.GetCart;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.UpdateCartItem;

public class Endpoint : Endpoint<UpdateCartItemRequest, CartResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Put("/api/cart/items/{productId}");

    public override async Task HandleAsync(UpdateCartItemRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new UpdateCartItemCommand(userId, req.ProductId, req.Quantity), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 400, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record UpdateCartItemRequest(Guid ProductId, int Quantity);
