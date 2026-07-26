namespace EventHub.Application.Features.Tickets.Queries.GetOrganizerDashboard;

public record OrganizerDashboardDto
{
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCancelledTickets { get; set; }
    public double CapacityUtilizationPercentage { get; set; } 
    public List<TicketTypeStatisticsDto> TicketTypeStats { get; set; } = new();
}

public record TicketTypeStatisticsDto
{
    public string TicketTypeName { get; set; } = string.Empty;
    public int SoldCount { get; set; }
    public int TotalCapacity { get; set; }
}