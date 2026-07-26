using EventHub.Domain.Common;
namespace EventHub.Domain.Entities;
public class Tag : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
}