using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Modules.Users.Features.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly AuthTokenIssuer _issuer;

    public LoginUserHandler(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, AuthTokenIssuer issuer)
    {
        _users = users; _signIn = signIn; _issuer = issuer;
    }

    public async Task<Result<AuthResponse>> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(cmd.Email);
        if (user is null || !user.IsActive)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        var result = await _signIn.CheckPasswordSignInAsync(user, cmd.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Result<AuthResponse>.Failure(result.IsLockedOut ? "Account locked. Try again later." : "Invalid email or password.");

        var roles = await _users.GetRolesAsync(user);
        var response = await _issuer.IssueAsync(user, roles, ct);
        return Result<AuthResponse>.Success(response);
    }
}
