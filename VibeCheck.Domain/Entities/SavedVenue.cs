using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

public class SavedVenue : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid VenueId { get; set; }

    public Venue Venue { get; set; } = null!;
}
