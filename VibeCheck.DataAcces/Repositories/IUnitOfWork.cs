using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Repositories;

public interface IUnitOfWork
{
    IGenericRepository<AppUser> Users { get; }

    IGenericRepository<RefreshToken> RefreshTokens { get; }

    IGenericRepository<Venue> Venues { get; }

    IGenericRepository<VibeCheckEntry> VibeCheckEntries { get; }

    IGenericRepository<VibeCheckPhoto> VibeCheckPhotos { get; }

    IGenericRepository<VibeTag> VibeTags { get; }

    IGenericRepository<VibeCheckEntryTag> VibeCheckEntryTags { get; }

    IGenericRepository<VibeCheckReaction> VibeCheckReactions { get; }

    IGenericRepository<VibeCheckComment> VibeCheckComments { get; }

    IGenericRepository<Follow> Follows { get; }

    IGenericRepository<SavedVenue> SavedVenues { get; }

    IGenericRepository<VenueAdminAssignment> VenueAdminAssignments { get; }

    IGenericRepository<VenueBan> VenueBans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
