namespace EventHub.Application.Features.Admin.Queries.GetPlatformStats;

public class PlatformStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalOrganizers { get; set; }
    public int TotalAttendees { get; set; }
    public int TotalEvents { get; set; }
    public int PublishedEvents { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}