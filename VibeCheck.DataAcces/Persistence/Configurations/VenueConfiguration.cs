using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.Property(v => v.Name).HasMaxLength(120).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(1000);
        builder.Property(v => v.Address).HasMaxLength(250).IsRequired();
        builder.Property(v => v.City).HasMaxLength(100).IsRequired();

        builder.HasIndex(v => v.Name);
        builder.HasIndex(v => v.City);
        builder.HasIndex(v => v.Category);

        builder.HasMany(v => v.VibeCheckEntries)
            .WithOne(e => e.Venue)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.SavedByUsers)
            .WithOne(s => s.Venue)
            .HasForeignKey(s => s.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
