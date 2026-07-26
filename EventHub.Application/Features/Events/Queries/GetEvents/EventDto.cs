using System;
using System.Collections.Generic;
using EventHub.Domain.Enums;

namespace EventHub.Application.Features.Events.Queries.GetEvents;

public class EventDto 
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public List<string> TagNames { get; set; } = new();
    
    public EventStatus Status { get; set; }
    public bool IsPrivate { get; set; }
    
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
}