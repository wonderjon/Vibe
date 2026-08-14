namespace VibeCheck.Domain.Common;

/// <summary>
/// Base type for every persisted entity: a GUID primary key plus a creation timestamp.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
