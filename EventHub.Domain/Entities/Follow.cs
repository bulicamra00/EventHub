using EventHub.Domain.Entities;

namespace EventHub.Domain.Entities;

public class Follow
{
    public Guid FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
}