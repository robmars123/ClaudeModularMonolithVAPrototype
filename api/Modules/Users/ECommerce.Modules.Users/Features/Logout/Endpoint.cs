using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Users.Features.Logout;

public class Endpoint : Endpoint<LogoutRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/logout");
        // The refresh token being revoked is the credential here, not a still-valid access token —
        // requiring auth would strand a client whose access token already expired but who still holds
        // (and needs to revoke) a valid refresh token.
        AllowAnonymous();
    }

    public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(req.RefreshToken), ct);
        await Send.NoContentAsync(ct);
    }
}

public record LogoutRequest(string RefreshToken);
