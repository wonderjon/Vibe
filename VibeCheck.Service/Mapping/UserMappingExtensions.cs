using VibeCheck.Domain.Entities;
using VibeCheck.Service.Dtos.Users;

namespace VibeCheck.Service.Mapping;

public static class UserMappingExtensions
{
    public static UserProfileDto ToProfileDto(this AppUser user, int followerCount, int followingCount, int vibeCheckCount)
        => new(user.Id, user.UserName, user.Email, user.DisplayName, user.AvatarUrl, user.Bio, user.Role, user.CreatedAt,
            followerCount, followingCount, vibeCheckCount);

    public static PublicUserDto ToPublicDto(this AppUser user, int followerCount, int followingCount, int vibeCheckCount, bool isFollowedByCurrentUser)
        => new(user.Id, user.UserName, user.DisplayName, user.AvatarUrl, user.Bio, user.CreatedAt,
            followerCount, followingCount, vibeCheckCount, isFollowedByCurrentUser);
}
