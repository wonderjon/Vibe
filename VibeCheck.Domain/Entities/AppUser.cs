using VibeCheck.Domain.Common;
using VibeCheck.Domain.Enums;

namespace VibeCheck.Domain.Entities;

public class AppUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    // Navigation
    public ICollection<Venue> CreatedVenues { get; set; } = new List<Venue>();

    public ICollection<VibeCheckEntry> VibeCheckEntries { get; set; } = new List<VibeCheckEntry>();

    public ICollection<VibeCheckComment> Comments { get; set; } = new List<VibeCheckComment>();

    public ICollection<VibeCheckReaction> Reactions { get; set; } = new List<VibeCheckReaction>();

    public ICollection<SavedVenue> SavedVenues { get; set; } = new List<SavedVenue>();

    public ICollection<Follow> Following { get; set; } = new List<Follow>();

    public ICollection<Follow> Followers { get; set; } = new List<Follow>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<VenueAdminAssignment> VenueAdminAssignments { get; set; } = new List<VenueAdminAssignment>();
}
