using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class Event : BaseEntity
{
    public Event()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = EventStatus.Draft;
    }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public string? OnlineLink { get; set; }
    public string? CoverImageUrl { get; set; }
    public EventStatus Status { get; private set; }
    
    public string? CancelReason { get; private set; }

    public bool IsBlocked { get; set; } = false;
    public string? BlockReason { get; private set; }
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;
    
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();

    public bool IsPrivate { get; set; } = false;
    public ICollection<EventInvitation> Invitations { get; set; } = new List<EventInvitation>();

    public Guid? EventSeriesId { get; set; }
    public EventSeries? EventSeries { get; set; }

    public void Publish()
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Nije moguće objaviti otkazan događaj.");
        
        if (Status == EventStatus.Published || Status == EventStatus.SoldOut)
            return;

        Status = EventStatus.Published;
    }

    public void Cancel(string reason)
    {
        if (Status == EventStatus.Cancelled)
            return;

        Status = EventStatus.Cancelled;
        CancelReason = reason;
    }

    public void Block(string reason)
    {
        IsBlocked = true;
        BlockReason = reason;
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }

    public void Complete()
    {
        if (Status == EventStatus.Completed)
            return;

        Status = EventStatus.Completed;
    }

    public void UpdateStatusIfSoldOut(int totalCapacity, int currentBookingsCount)
    {
        if (Status == EventStatus.Published && currentBookingsCount >= totalCapacity)
        {
            Status = EventStatus.SoldOut;
        }
    }

    public bool IsSaleActive()
    {
        return Status == EventStatus.Published && !IsBlocked && StartDate > DateTime.UtcNow.AddHours(1);
    }
}