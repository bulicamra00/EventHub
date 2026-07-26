using EventHub.Domain.Enums;
using System;
using System.Collections.Generic;

namespace EventHub.Application.Features.Events.Queries.GetEventDetails;

public class EventDetailsDto 
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? OnlineLink { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public List<string> TagNames { get; set; } = new();
    
    public bool IsBookable { get; set; }

    public bool UserHasTicket { get; set; }

    public EventStatus Status { get; set; }

    public string? CancelReason { get; set; }
    
    public List<TicketTypeDto> TicketTypes { get; set; } = new();
}

public class TicketTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableCapacity { get; set; }
}