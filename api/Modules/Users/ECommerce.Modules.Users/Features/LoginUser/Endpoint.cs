using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Users.Features.LoginUser;

public class Endpoint : Endpoint<LoginUserCommand, AuthResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginUserCommand cmd, CancellationToken ct)
    {
        Result<AuthResponse> result = await _mediator.Send(cmd, ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { errors = result.Errors }, 400, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}
