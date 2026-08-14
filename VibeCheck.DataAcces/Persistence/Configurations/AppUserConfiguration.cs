using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.UserName).HasMaxLength(32).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(64).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(280);

        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.CreatedVenues)
            .WithOne(v => v.CreatedByUser)
            .HasForeignKey(v => v.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting an Admin account cleans up their venue assignments; deleting a venue (Restrict,
        // configured on the Venue side) must not silently delete the admin who manages it.
        builder.HasMany(u => u.VenueAdminAssignments)
            .WithOne(a => a.AdminUser)
            .HasForeignKey(a => a.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
