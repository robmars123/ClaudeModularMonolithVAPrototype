using ECommerce.Modules.Customers.Features.GetMyProfile;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Customers.Features.UpdateMyProfile;

public record UpdateMyProfileCommand(string UserId, string FirstName, string LastName, string? Phone)
    : IRequest<Result<CustomerResponse>>;
