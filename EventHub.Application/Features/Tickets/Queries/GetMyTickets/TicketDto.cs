namespace EventHub.Application.Features.Tickets.Queries.GetMyTickets;

public class TicketDto
{
    public Guid Id { get; set; }
    
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } 
    
    public string AttendeeName { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    
    public string Status { get; set; } = string.Empty;

    public string QrCodeBase64 { get; set; } = string.Empty;
}