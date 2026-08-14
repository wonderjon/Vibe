using VibeCheck.Domain.Enums;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;

namespace VibeCheck.Service.Interfaces;

public interface IVibeCheckService
{
    Task<VibeCheckDto> CreateAsync(Guid currentUserId, CreateVibeCheckRequest request, CancellationToken cancellationToken = default);

    Task<VibeCheckDto> GetByIdAsync(Guid id, Guid? currentUserId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid currentUserId, Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<VibeCheckDto>> GetFollowingFeedAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<VibeCheckDto>> GetGlobalFeedAsync(Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Toggles a reaction: adding it if absent/different, removing it if the same type is reacted again.</summary>
    Task ReactAsync(Guid currentUserId, Guid entryId, ReactionType type, CancellationToken cancellationToken = default);

    Task<VibeCheckCommentDto> AddCommentAsync(Guid currentUserId, Guid entryId, CreateCommentRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<VibeCheckCommentDto>> GetCommentsAsync(Guid entryId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task DeleteCommentAsync(Guid currentUserId, Guid commentId, CancellationToken cancellationToken = default);
}
