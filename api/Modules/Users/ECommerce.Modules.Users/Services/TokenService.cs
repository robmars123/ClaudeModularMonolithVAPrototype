using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Modules.Users.Entities;
using ECommerce.Shared.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Modules.Users.Services;

public class TokenService
{
    private readonly JwtOptions _options;
    public TokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public double AccessTokenExpiryMinutes => _options.AccessTokenExpiryMinutes;
    public double RefreshTokenExpiryDays => _options.RefreshTokenExpiryDays;

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = Encoding.UTF8.GetBytes(_options.Secret);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Cryptographically random opaque refresh token. Returns the raw value (sent to the client, never stored) and its hash (persisted).</summary>
    public (string RawToken, string Hash) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (raw, Hash(raw));
    }

    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
