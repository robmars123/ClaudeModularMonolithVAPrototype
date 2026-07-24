using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Customers.Features.GetMyProfile;

public class Endpoint : EndpointWithoutRequest<CustomerResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Get("/api/customers/me");

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetMyProfileQuery(userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}
