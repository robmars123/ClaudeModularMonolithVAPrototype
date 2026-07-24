using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
