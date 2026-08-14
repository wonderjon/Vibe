using VibeCheck.Domain.Enums;

namespace VibeCheck.Service.Dtos.Users;

/// <summary>The current user's own full profile (returned from /users/me and auth responses).</summary>
public record UserProfileDto(
    Guid Id,
    string UserName,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    UserRole Role,
    DateTime CreatedAt,
    int FollowerCount,
    int FollowingCount,
    int VibeCheckCount);

/// <summary>Another user's public profile.</summary>
public record PublicUserDto(
    Guid Id,
    string UserName,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    DateTime CreatedAt,
    int FollowerCount,
    int FollowingCount,
    int VibeCheckCount,
    bool IsFollowedByCurrentUser);

public record UpdateProfileRequest(string DisplayName, string? Bio, string? AvatarUrl);
