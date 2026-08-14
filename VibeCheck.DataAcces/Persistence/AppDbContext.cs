using Microsoft.EntityFrameworkCore;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<VibeCheckEntry> VibeCheckEntries => Set<VibeCheckEntry>();

    public DbSet<VibeCheckPhoto> VibeCheckPhotos => Set<VibeCheckPhoto>();

    public DbSet<VibeTag> VibeTags => Set<VibeTag>();

    public DbSet<VibeCheckEntryTag> VibeCheckEntryTags => Set<VibeCheckEntryTag>();

    public DbSet<VibeCheckReaction> VibeCheckReactions => Set<VibeCheckReaction>();

    public DbSet<VibeCheckComment> VibeCheckComments => Set<VibeCheckComment>();

    public DbSet<Follow> Follows => Set<Follow>();

    public DbSet<SavedVenue> SavedVenues => Set<SavedVenue>();

    public DbSet<VenueAdminAssignment> VenueAdminAssignments => Set<VenueAdminAssignment>();

    public DbSet<VenueBan> VenueBans => Set<VenueBan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
