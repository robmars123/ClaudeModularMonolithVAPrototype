using Microsoft.Extensions.Options;

namespace ECommerce.Shared.Common;

/// <summary>Fails fast at startup (via ValidateOnStart) instead of surfacing a bad JWT config on the first login/token issuance.</summary>
public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Secret) || options.Secret.Length < 32)
            errors.Add($"{JwtOptions.SectionName}:Secret must be at least 32 characters.");
        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add($"{JwtOptions.SectionName}:Issuer is required.");
        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add($"{JwtOptions.SectionName}:Audience is required.");
        if (options.AccessTokenExpiryMinutes <= 0)
            errors.Add($"{JwtOptions.SectionName}:AccessTokenExpiryMinutes must be positive.");
        if (options.RefreshTokenExpiryDays <= 0)
            errors.Add($"{JwtOptions.SectionName}:RefreshTokenExpiryDays must be positive.");

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
