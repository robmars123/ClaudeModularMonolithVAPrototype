using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Users.Features.CreateStaffUser;

/// <summary>Admin-only staff provisioning — the only way to grant a non-Customer role. Public registration cannot self-select a role.</summary>
public record CreateStaffUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role) : IRequest<Result<StaffUserResponse>>, IValidatableRequest
{
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(FirstName)) yield return "First name is required.";
        if (string.IsNullOrWhiteSpace(LastName)) yield return "Last name is required.";
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@')) yield return "A valid email is required.";
        if (string.IsNullOrWhiteSpace(Password)) yield return "Password is required.";
        if (!Roles.Staff.Contains(Role)) yield return $"Role must be one of: {string.Join(", ", Roles.Staff)}.";
    }
}

public record StaffUserResponse(string Id, string Email, string FullName, string Role);
