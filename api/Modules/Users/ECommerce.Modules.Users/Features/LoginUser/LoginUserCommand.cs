using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<AuthResponse>>, IValidatableRequest
{
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Email)) yield return "Email is required.";
        if (string.IsNullOrWhiteSpace(Password)) yield return "Password is required.";
    }
}
