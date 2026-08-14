using VibeCheck.Domain.Entities;

namespace VibeCheck.Service.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user);

    string GenerateRefreshTokenValue();

    /// <summary>Self-contained, short-lived token used for the forgot/reset password flow — no DB row needed.</summary>
    string GeneratePasswordResetToken(Guid userId);

    /// <summary>Returns the user id if the token is a valid, unexpired password-reset token; otherwise null.</summary>
    Guid? ValidatePasswordResetToken(string token);
}
