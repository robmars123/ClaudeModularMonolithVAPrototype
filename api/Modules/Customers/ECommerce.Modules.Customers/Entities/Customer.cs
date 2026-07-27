using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Customers.Entities;

public class Customer : AuditableEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public bool IsActive { get; private set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();

    private Customer() { }

    public static Customer Create(string userId, string firstName, string lastName, string email)
    {
        Customer c = new Customer { UserId = userId, FirstName = firstName, LastName = lastName, Email = email };
        c.SetCreatedBy(userId);
        return c;
    }

    public void UpdateProfile(string firstName, string lastName, string? phone, string updatedBy)
    {
        FirstName = firstName; LastName = lastName; Phone = phone;
        SetUpdatedBy(updatedBy);
    }

    public void UpdateShippingAddress(Address address, string updatedBy)
    {
        ShippingAddress = address;
        SetUpdatedBy(updatedBy);
    }
}

public record Address(string Street, string City, string State, string ZipCode, string Country);
