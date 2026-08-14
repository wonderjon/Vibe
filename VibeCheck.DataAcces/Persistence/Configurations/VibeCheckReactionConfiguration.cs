using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VibeCheckReactionConfiguration : IEntityTypeConfiguration<VibeCheckReaction>
{
    public void Configure(EntityTypeBuilder<VibeCheckReaction> builder)
    {
        builder.HasIndex(r => new { r.VibeCheckEntryId, r.UserId }).IsUnique();

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reactions)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
