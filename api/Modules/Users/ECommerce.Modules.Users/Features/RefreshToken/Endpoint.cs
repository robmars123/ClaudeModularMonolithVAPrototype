using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Users.Features.RefreshToken;

public class Endpoint : Endpoint<RefreshTokenRequest, RegisterUser.AuthResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/refresh");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        Result<AuthResponse> result = await _mediator.Send(new RefreshTokenCommand(req.RefreshToken), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { errors = result.Errors }, 401, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record RefreshTokenRequest(string RefreshToken);
