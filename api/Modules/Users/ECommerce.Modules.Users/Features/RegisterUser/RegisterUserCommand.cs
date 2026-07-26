using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<AuthResponse>>, IValidatableRequest
{
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(FirstName)) yield return "First name is required.";
        if (string.IsNullOrWhiteSpace(LastName)) yield return "Last name is required.";
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@')) yield return "A valid email is required.";
        if (string.IsNullOrWhiteSpace(Password)) yield return "Password is required.";
    }
}

public record AuthResponse(
    string Token,
    string RefreshToken,
    string Email,
    string FullName,
    IList<string> Roles,
    DateTime ExpiresAt);
