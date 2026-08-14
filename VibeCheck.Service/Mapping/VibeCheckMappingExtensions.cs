using VibeCheck.Domain.Entities;
using VibeCheck.Domain.Enums;
using VibeCheck.Service.Dtos.VibeChecks;

namespace VibeCheck.Service.Mapping;

public static class VibeCheckMappingExtensions
{
    /// <summary>
    /// Maps an entry to its DTO. Expects Venue, User, Photos, EntryTags(.VibeTag), Reactions and Comments
    /// to already be loaded (or empty collections) by the caller's query.
    /// </summary>
    public static VibeCheckDto ToDto(this VibeCheckEntry entry, Guid? currentUserId)
    {
        var currentUserReaction = currentUserId is null
            ? null
            : entry.Reactions.FirstOrDefault(r => r.UserId == currentUserId)?.Type;

        return new VibeCheckDto(
            entry.Id,
            entry.VenueId,
            entry.Venue?.Name ?? string.Empty,
            entry.UserId,
            entry.User?.UserName ?? string.Empty,
            entry.User?.AvatarUrl,
            entry.VibeScore,
            entry.CrowdLevel,
            entry.Comment,
            entry.CreatedAt,
            entry.ExpiresAt,
            entry.IsActive,
            entry.Photos.Select(p => p.Url).ToList(),
            entry.EntryTags.Select(t => t.VibeTag?.Name ?? string.Empty).Where(n => n.Length > 0).ToList(),
            entry.Reactions.Count(r => r.Type == ReactionType.Like),
            entry.Reactions.Count(r => r.Type == ReactionType.Fire),
            entry.Reactions.Count(r => r.Type == ReactionType.Boring),
            entry.Comments.Count,
            currentUserReaction);
    }

    public static VibeCheckCommentDto ToDto(this VibeCheckComment comment)
        => new(comment.Id, comment.UserId, comment.User?.UserName ?? string.Empty, comment.User?.AvatarUrl, comment.Text, comment.CreatedAt);
}
