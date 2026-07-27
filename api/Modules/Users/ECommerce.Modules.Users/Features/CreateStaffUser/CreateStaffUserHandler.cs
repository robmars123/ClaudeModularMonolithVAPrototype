using ECommerce.Modules.Users.Entities;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Modules.Users.Features.CreateStaffUser;

public class CreateStaffUserHandler : IRequestHandler<CreateStaffUserCommand, Result<StaffUserResponse>>
{
    private readonly UserManager<ApplicationUser> _users;
    public CreateStaffUserHandler(UserManager<ApplicationUser> users) => _users = users;

    public async Task<Result<StaffUserResponse>> Handle(CreateStaffUserCommand cmd, CancellationToken ct)
    {
        if (await _users.FindByEmailAsync(cmd.Email) is not null)
            return Result<StaffUserResponse>.Failure("Email already registered.");

        ApplicationUser user = new ApplicationUser { FirstName = cmd.FirstName, LastName = cmd.LastName, Email = cmd.Email, UserName = cmd.Email, EmailConfirmed = true };

        IdentityResult result = await _users.CreateAsync(user, cmd.Password);
        if (!result.Succeeded)
            return Result<StaffUserResponse>.Failure(result.Errors.Select(e => e.Description).ToList());

        IdentityResult roleResult = await _users.AddToRoleAsync(user, cmd.Role);
        if (!roleResult.Succeeded)
        {
            // The user account exists but has no role — leaving it half-provisioned is worse than
            // deleting it and reporting failure, since a roleless account can still authenticate.
            await _users.DeleteAsync(user);
            return Result<StaffUserResponse>.Failure(roleResult.Errors.Select(e => e.Description).ToList());
        }

        // Deliberately no UserRegisteredEvent here — that event drives Customers' auto-created profile,
        // which staff accounts (Admin/ProductManager/Support) must not get.
        return Result<StaffUserResponse>.Success(new StaffUserResponse(user.Id, user.Email!, user.FullName, cmd.Role));
    }
}
