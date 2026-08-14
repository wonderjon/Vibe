using VibeCheck.Domain.Enums;

namespace VibeCheck.Service.Dtos.VibeChecks;

public record VibeCheckDto(
    Guid Id,
    Guid VenueId,
    string VenueName,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    int VibeScore,
    CrowdLevel CrowdLevel,
    string? Comment,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive,
    IReadOnlyList<string> PhotoUrls,
    IReadOnlyList<string> Tags,
    int LikeCount,
    int FireCount,
    int BoringCount,
    int CommentCount,
    ReactionType? CurrentUserReaction);

public record CreateVibeCheckRequest(
    Guid VenueId,
    int VibeScore,
    CrowdLevel CrowdLevel,
    string? Comment,
    IReadOnlyList<string>? PhotoUrls,
    IReadOnlyList<Guid>? TagIds);

public record VibeCheckCommentDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    string Text,
    DateTime CreatedAt);

public record CreateCommentRequest(string Text);

public record ReactRequest(ReactionType Type);
