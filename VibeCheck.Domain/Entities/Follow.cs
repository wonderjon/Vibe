using VibeCheck.Domain.Common;

namespace VibeCheck.Domain.Entities;

public class Follow : BaseEntity
{
    public Guid FollowerId { get; set; }

    public AppUser Follower { get; set; } = null!;

    public Guid FollowingId { get; set; }

    public AppUser FollowingUser { get; set; } = null!;
}
