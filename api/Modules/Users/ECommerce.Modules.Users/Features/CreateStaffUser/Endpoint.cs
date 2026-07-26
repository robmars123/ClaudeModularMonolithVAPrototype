using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Users.Features.CreateStaffUser;

public class Endpoint : Endpoint<CreateStaffUserRequest, StaffUserResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/staff");
        Roles(ECommerce.Shared.Common.Roles.Admin);
    }

    public override async Task HandleAsync(CreateStaffUserRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateStaffUserCommand(req.FirstName, req.LastName, req.Email, req.Password, req.Role), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { errors = result.Errors }, 400, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record CreateStaffUserRequest(string FirstName, string LastName, string Email, string Password, string Role);
