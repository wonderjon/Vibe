using VibeCheck.Domain.Common;
using VibeCheck.Domain.Enums;

namespace VibeCheck.Domain.Entities;

/// <summary>
/// A single "vibe check" — a user's live rating of a venue's current atmosphere.
/// Vibe checks are inherently time-sensitive: they matter for a few hours, then age out.
/// </summary>
public class VibeCheckEntry : BaseEntity
{
    public Guid VenueId { get; set; }

    public Venue Venue { get; set; } = null!;

    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public int VibeScore { get; set; } // 1-5

    public CrowdLevel CrowdLevel { get; set; }

    public string? Comment { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsActive => DateTime.UtcNow < ExpiresAt;

    // Navigation
    public ICollection<VibeCheckPhoto> Photos { get; set; } = new List<VibeCheckPhoto>();

    public ICollection<VibeCheckEntryTag> EntryTags { get; set; } = new List<VibeCheckEntryTag>();

    public ICollection<VibeCheckReaction> Reactions { get; set; } = new List<VibeCheckReaction>();

    public ICollection<VibeCheckComment> Comments { get; set; } = new List<VibeCheckComment>();
}
