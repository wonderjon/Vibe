using VibeCheck.Domain.Common;
using VibeCheck.Domain.Enums;

namespace VibeCheck.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public VenueCategory Category { get; set; }

    public string? Description { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? CoverImageUrl { get; set; }

    public Guid CreatedByUserId { get; set; }

    public AppUser CreatedByUser { get; set; } = null!;

    // Denormalized aggregates, recomputed whenever a vibe check is added/expires.
    public double AverageVibeScore { get; set; }

    public int TotalCheckIns { get; set; }

    // Navigation
    public ICollection<VibeCheckEntry> VibeCheckEntries { get; set; } = new List<VibeCheckEntry>();

    public ICollection<SavedVenue> SavedByUsers { get; set; } = new List<SavedVenue>();

    public ICollection<VenueAdminAssignment> AdminAssignments { get; set; } = new List<VenueAdminAssignment>();

    public ICollection<VenueBan> Bans { get; set; } = new List<VenueBan>();
}
