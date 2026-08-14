using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VibeCheck.Domain.Entities;

namespace VibeCheck.Service.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private const string PasswordResetPurposeClaim = "purpose";
    private const string PasswordResetPurposeValue = "password_reset";

    public string GeneratePasswordResetToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(PasswordResetPurposeClaim, PasswordResetPurposeValue)
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidatePasswordResetToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.Key));

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);

            var purpose = principal.FindFirstValue(PasswordResetPurposeClaim);
            if (purpose != PasswordResetPurposeValue)
                return null;

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
