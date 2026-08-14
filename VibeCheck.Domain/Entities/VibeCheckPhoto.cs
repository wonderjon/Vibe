using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

public class VibeCheckPhoto : BaseEntity
{
    public Guid VibeCheckEntryId { get; set; }

    public VibeCheckEntry VibeCheckEntry { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
}
