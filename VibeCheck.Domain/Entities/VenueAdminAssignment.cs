using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

/// <summary>Which Admin accounts manage which venues — an Admin only sees/manages venues assigned here.</summary>
public class VenueAdminAssignment : BaseEntity
{
    public Guid AdminUserId { get; set; }

    public AppUser AdminUser { get; set; } = null!;

    public Guid VenueId { get; set; }

    public Venue Venue { get; set; } = null!;
}
