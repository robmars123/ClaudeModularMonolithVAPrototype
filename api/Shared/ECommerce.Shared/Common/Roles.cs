namespace ECommerce.Shared.Common;

/// <summary>Canonical role names — the only place role strings should be spelled out.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string ProductManager = "ProductManager";
    public const string Support = "Support";
    public const string Customer = "Customer";

    /// <summary>Roles that may be self-selected at public registration. Staff roles are provisioned separately by an Admin.</summary>
    public static readonly string[] SelfRegisterable = [Customer];

    /// <summary>Roles an Admin may grant when provisioning a staff account.</summary>
    public static readonly string[] Staff = [Admin, ProductManager, Support];

    public static readonly string[] All = [Admin, ProductManager, Support, Customer];
}
