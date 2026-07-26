namespace EventHub.Application.Features.Tickets.Queries.GetAttendees;

public record AttendeeDto
{
    public Guid TicketId { get; set; }
    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public bool IsScanned { get; set; } 
}