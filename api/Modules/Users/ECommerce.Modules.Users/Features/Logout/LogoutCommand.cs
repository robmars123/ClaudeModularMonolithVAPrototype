using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.Logout;

public record LogoutCommand(string RefreshTokenValue) : IRequest<Result>;
