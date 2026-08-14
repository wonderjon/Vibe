namespace VibeCheck.Domain.Entities;

/// <summary>
/// Join entity: many-to-many between VibeCheckEntry and VibeTag.
/// </summary>
public class VibeCheckEntryTag
{
    public Guid VibeCheckEntryId { get; set; }

    public VibeCheckEntry VibeCheckEntry { get; set; } = null!;

    public Guid VibeTagId { get; set; }

    public VibeTag VibeTag { get; set; } = null!;
}
