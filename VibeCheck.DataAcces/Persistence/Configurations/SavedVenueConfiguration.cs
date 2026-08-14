using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class SavedVenueConfiguration : IEntityTypeConfiguration<SavedVenue>
{
    public void Configure(EntityTypeBuilder<SavedVenue> builder)
    {
        builder.HasIndex(s => new { s.UserId, s.VenueId }).IsUnique();

        builder.HasOne(s => s.User)
            .WithMany(u => u.SavedVenues)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
