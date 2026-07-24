using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.GetUserProfile;

public record GetUserProfileQuery(string UserId) : IRequest<Result<UserProfileResponse>>;

public record UserProfileResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IList<string> Roles,
    DateTime CreatedAt);
