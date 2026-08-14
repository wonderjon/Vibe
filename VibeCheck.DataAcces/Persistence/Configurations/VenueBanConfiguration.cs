using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VenueBanConfiguration : IEntityTypeConfiguration<VenueBan>
{
    public void Configure(EntityTypeBuilder<VenueBan> builder)
    {
        builder.Property(b => b.Reason).HasMaxLength(500);

        builder.HasIndex(b => new { b.VenueId, b.UserId }).IsUnique();

        // Deleting the venue (SuperAdmin only) takes its ban list with it.
        builder.HasOne(b => b.Venue)
            .WithMany(v => v.Bans)
            .HasForeignKey(b => b.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict on both AppUser FK directions (the banned user, and the admin who issued the
        // ban) — Follow already established this pattern for multiple FKs into AppUser on one
        // entity, to avoid cascade ambiguity when deleting a user account.
        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull (not Restrict) here specifically: the ban itself should stay in effect even if
        // the admin who issued it is later removed — we just lose the "who banned them" attribution.
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(b => b.BannedByAdminUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
