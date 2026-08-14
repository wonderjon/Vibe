using VibeCheck.Domain.Common;
using VibeCheck.Domain.Enums;

namespace VibeCheck.Domain.Entities;

public class VibeCheckReaction : BaseEntity
{
    public Guid VibeCheckEntryId { get; set; }

    public VibeCheckEntry VibeCheckEntry { get; set; } = null!;

    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public ReactionType Type { get; set; }
}
