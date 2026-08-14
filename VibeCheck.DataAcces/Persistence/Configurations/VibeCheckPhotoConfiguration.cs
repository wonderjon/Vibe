using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence.Configurations;

public class VibeCheckPhotoConfiguration : IEntityTypeConfiguration<VibeCheckPhoto>
{
    public void Configure(EntityTypeBuilder<VibeCheckPhoto> builder)
    {
        builder.Property(p => p.Url).HasMaxLength(500).IsRequired();
    }
}
