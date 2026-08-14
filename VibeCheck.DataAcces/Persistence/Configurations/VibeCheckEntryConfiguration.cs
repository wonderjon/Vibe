using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VibeCheckEntryConfiguration : IEntityTypeConfiguration<VibeCheckEntry>
{
    public void Configure(EntityTypeBuilder<VibeCheckEntry> builder)
    {
        builder.Property(e => e.Comment).HasMaxLength(500);

        builder.HasIndex(e => e.VenueId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasOne(e => e.User)
            .WithMany(u => u.VibeCheckEntries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Photos)
            .WithOne(p => p.VibeCheckEntry)
            .HasForeignKey(p => p.VibeCheckEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Reactions)
            .WithOne(r => r.VibeCheckEntry)
            .HasForeignKey(r => r.VibeCheckEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.VibeCheckEntry)
            .HasForeignKey(c => c.VibeCheckEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EntryTags)
            .WithOne(t => t.VibeCheckEntry)
            .HasForeignKey(t => t.VibeCheckEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
