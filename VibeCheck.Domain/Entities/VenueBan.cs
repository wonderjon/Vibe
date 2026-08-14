using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

/// <summary>A user blocked from posting vibe checks at a specific venue, set by that venue's Admin/SuperAdmin.</summary>
public class VenueBan : BaseEntity
{
    public Guid VenueId { get; set; }

    public Venue Venue { get; set; } = null!;

    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    /// <summary>Nullable so a ban survives the issuing admin's account later being deleted.</summary>
    public Guid? BannedByAdminUserId { get; set; }

    public string? Reason { get; set; }
}
