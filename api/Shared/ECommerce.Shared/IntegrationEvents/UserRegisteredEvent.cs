using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.IntegrationEvents;

/// <summary>Published by Users after a successful registration. Customers subscribes to create the profile.</summary>
public sealed record UserRegisteredEvent(string UserId, string Email, string FirstName, string LastName) : INotification
{
    /// <summary>Schema version of this event's shape, so a consumer can branch on it if the shape ever changes.</summary>
    public int Version { get; init; } = 1;
}
