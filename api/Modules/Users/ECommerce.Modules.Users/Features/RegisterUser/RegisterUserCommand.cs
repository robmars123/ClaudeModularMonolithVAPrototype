using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role = "Customer") : IRequest<Result<AuthResponse>>;

public record AuthResponse(
    string Token,
    string Email,
    string FullName,
    IList<string> Roles,
    DateTime ExpiresAt);
