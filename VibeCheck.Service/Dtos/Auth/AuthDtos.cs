using VibeCheck.Service.Dtos.Users;

namespace VibeCheck.Service.Dtos.Auth;

public record RegisterRequest(string UserName, string Email, string Password, string DisplayName);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string ResetToken, string NewPassword);

public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, UserProfileDto User);
