using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VibeCheckEntryTagConfiguration : IEntityTypeConfiguration<VibeCheckEntryTag>
{
    public void Configure(EntityTypeBuilder<VibeCheckEntryTag> builder)
    {
        builder.HasKey(t => new { t.VibeCheckEntryId, t.VibeTagId });

        builder.HasOne(t => t.VibeTag)
            .WithMany(vt => vt.EntryTags)
            .HasForeignKey(t => t.VibeTagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
