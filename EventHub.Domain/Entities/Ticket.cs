using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class Ticket : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid TicketTypeId { get; set; }
    public TicketType TicketType { get; set; } = null!;

    public string TicketCode { get; set; } = Guid.NewGuid().ToString();
    public decimal PurchasePrice { get; set; } 
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    
    public TicketStatus Status { get; set; }

    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;

    public bool ReminderSent { get; set; } = false;
    public DateTime? ReminderSentAt { get; set; }
}