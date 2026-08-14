using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.Users;
using VibeCheck.Service.Dtos.VibeChecks;

namespace VibeCheck.Service.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task<PublicUserDto> GetPublicProfileAsync(Guid targetUserId, Guid? currentUserId, CancellationToken cancellationToken = default);

    Task<PagedResult<VibeCheckDto>> GetUserVibeChecksAsync(Guid targetUserId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<PublicUserDto>> SearchUsersAsync(string query, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task FollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);

    Task UnfollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);

    Task<PagedResult<PublicUserDto>> GetFollowersAsync(Guid targetUserId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<PublicUserDto>> GetFollowingAsync(Guid targetUserId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
}
