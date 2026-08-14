using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

public class VibeCheckComment : BaseEntity
{
    public Guid VibeCheckEntryId { get; set; }

    public VibeCheckEntry VibeCheckEntry { get; set; } = null!;

    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
}
