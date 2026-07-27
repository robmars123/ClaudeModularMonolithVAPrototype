using System.Security.Claims;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Users.Features.GetUserProfile;

public class Endpoint : EndpointWithoutRequest<UserProfileResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Get("/api/auth/me");

    public override async Task HandleAsync(CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Result<UserProfileResponse> result = await _mediator.Send(new GetUserProfileQuery(userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}
