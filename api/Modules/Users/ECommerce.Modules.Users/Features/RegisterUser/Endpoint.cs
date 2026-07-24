using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Users.Features.RegisterUser;

public class Endpoint : Endpoint<RegisterUserCommand, AuthResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { errors = result.Errors }, 400, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}
