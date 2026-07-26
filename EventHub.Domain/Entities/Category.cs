using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class Category : BaseEntity
{
    public Category()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<Event> Events { get; set; } = new List<Event>();
}