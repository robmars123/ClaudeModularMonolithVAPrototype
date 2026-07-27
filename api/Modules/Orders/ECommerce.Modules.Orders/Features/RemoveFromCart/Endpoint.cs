using System.Security.Claims;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.RemoveFromCart;

public class Endpoint : Endpoint<RemoveFromCartRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Delete("/api/cart/items/{productId}");

    public override async Task HandleAsync(RemoveFromCartRequest req, CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Result result = await _mediator.Send(new RemoveFromCartCommand(userId, req.ProductId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.NoContentAsync(ct);
    }
}

public record RemoveFromCartRequest(Guid ProductId);
