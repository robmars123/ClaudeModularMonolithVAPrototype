using System.Security.Claims;
using ECommerce.Modules.Customers.Features.GetMyProfile;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Customers.Features.UpdateMyProfile;

public class Endpoint : Endpoint<UpdateProfileRequest, CustomerResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Put("/api/customers/me");

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Result<CustomerResponse> result = await _mediator.Send(new UpdateMyProfileCommand(userId, req.FirstName, req.LastName, req.Phone), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string? Phone);
