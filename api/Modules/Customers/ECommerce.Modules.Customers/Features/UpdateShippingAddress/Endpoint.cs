using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Customers.Features.UpdateShippingAddress;

public class Endpoint : Endpoint<AddressRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Put("/api/customers/me/shipping-address");

    public override async Task HandleAsync(AddressRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new UpdateShippingAddressCommand(userId, req.Street, req.City, req.State, req.ZipCode, req.Country), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(ct);
    }
}

public record AddressRequest(string Street, string City, string State, string ZipCode, string Country);
