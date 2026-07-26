namespace EventHub.Application.Features.Bookings.Queries.GetMyBookings;

public class BookingDto
{
    public Guid Id { get; set; }
    
    public string? EventTitle { get; set; } 
    public Guid TicketTypeId { get; set; }
    public DateTime CreatedAt { get; set; } 
    
    public int Quantity { get; set; }
    
    public string? Status { get; set; }
    
    public decimal TotalPrice { get; set; }
}