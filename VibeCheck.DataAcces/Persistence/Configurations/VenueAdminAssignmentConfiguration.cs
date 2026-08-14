using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VenueAdminAssignmentConfiguration : IEntityTypeConfiguration<VenueAdminAssignment>
{
    public void Configure(EntityTypeBuilder<VenueAdminAssignment> builder)
    {
        builder.HasIndex(a => new { a.AdminUserId, a.VenueId }).IsUnique();

        // Both sides cascade: deleting the admin's account, or deleting the venue itself
        // (SuperAdmin only), removes just the assignment rows tied to that specific
        // admin/venue — an admin's assignments to other venues are untouched.
        builder.HasOne(a => a.Venue)
            .WithMany(v => v.AdminAssignments)
            .HasForeignKey(a => a.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
