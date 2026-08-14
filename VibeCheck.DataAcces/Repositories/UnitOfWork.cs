using VibeCheck.DataAcces.Persistence;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new GenericRepository<AppUser>(_context);
        RefreshTokens = new GenericRepository<RefreshToken>(_context);
        Venues = new GenericRepository<Venue>(_context);
        VibeCheckEntries = new GenericRepository<VibeCheckEntry>(_context);
        VibeCheckPhotos = new GenericRepository<VibeCheckPhoto>(_context);
        VibeTags = new GenericRepository<VibeTag>(_context);
        VibeCheckEntryTags = new GenericRepository<VibeCheckEntryTag>(_context);
        VibeCheckReactions = new GenericRepository<VibeCheckReaction>(_context);
        VibeCheckComments = new GenericRepository<VibeCheckComment>(_context);
        Follows = new GenericRepository<Follow>(_context);
        SavedVenues = new GenericRepository<SavedVenue>(_context);
        VenueAdminAssignments = new GenericRepository<VenueAdminAssignment>(_context);
        VenueBans = new GenericRepository<VenueBan>(_context);
    }

    public IGenericRepository<AppUser> Users { get; }

    public IGenericRepository<RefreshToken> RefreshTokens { get; }

    public IGenericRepository<Venue> Venues { get; }

    public IGenericRepository<VibeCheckEntry> VibeCheckEntries { get; }

    public IGenericRepository<VibeCheckPhoto> VibeCheckPhotos { get; }

    public IGenericRepository<VibeTag> VibeTags { get; }

    public IGenericRepository<VibeCheckEntryTag> VibeCheckEntryTags { get; }

    public IGenericRepository<VibeCheckReaction> VibeCheckReactions { get; }

    public IGenericRepository<VibeCheckComment> VibeCheckComments { get; }

    public IGenericRepository<Follow> Follows { get; }

    public IGenericRepository<SavedVenue> SavedVenues { get; }

    public IGenericRepository<VenueAdminAssignment> VenueAdminAssignments { get; }

    public IGenericRepository<VenueBan> VenueBans { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
