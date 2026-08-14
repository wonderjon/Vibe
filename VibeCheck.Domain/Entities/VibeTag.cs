using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

/// <summary>
/// Lookup table of selectable vibe descriptors (e.g. "Live Music", "Long Line", "Cheap Drinks").
/// </summary>
public class VibeTag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<VibeCheckEntryTag> EntryTags { get; set; } = new List<VibeCheckEntryTag>();
}
