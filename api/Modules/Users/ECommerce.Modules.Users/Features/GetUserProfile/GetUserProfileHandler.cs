using ECommerce.Modules.Users.Entities;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Modules.Users.Features.GetUserProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileResponse>>
{
    private readonly UserManager<ApplicationUser> _users;
    public GetUserProfileHandler(UserManager<ApplicationUser> users) => _users = users;

    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        ApplicationUser? user = await _users.FindByIdAsync(query.UserId);
        if (user is null) return Result<UserProfileResponse>.Failure("User not found.");

        IList<string> roles = await _users.GetRolesAsync(user);
        return Result<UserProfileResponse>.Success(new UserProfileResponse(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName, roles, user.CreatedAt));
    }
}
