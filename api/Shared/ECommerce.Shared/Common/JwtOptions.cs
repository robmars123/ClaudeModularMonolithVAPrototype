namespace ECommerce.Shared.Common;

/// <summary>Bound from the "JwtSettings" configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public double AccessTokenExpiryMinutes { get; set; } = 15;
    public double RefreshTokenExpiryDays { get; set; } = 7;
}
