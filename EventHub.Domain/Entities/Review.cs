using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}