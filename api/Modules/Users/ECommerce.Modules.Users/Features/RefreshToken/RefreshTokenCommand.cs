using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.RefreshToken;

public record RefreshTokenCommand(string RefreshTokenValue) : IRequest<Result<AuthResponse>>, IValidatableRequest
{
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(RefreshTokenValue)) yield return "Refresh token is required.";
    }
}
