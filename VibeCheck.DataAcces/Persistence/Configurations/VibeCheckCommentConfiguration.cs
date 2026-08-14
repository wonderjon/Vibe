using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VibeCheckCommentConfiguration : IEntityTypeConfiguration<VibeCheckComment>
{
    public void Configure(EntityTypeBuilder<VibeCheckComment> builder)
    {
        builder.Property(c => c.Text).HasMaxLength(500).IsRequired();

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
