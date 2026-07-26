using System;
using System.Collections.Generic;

namespace EventHub.Domain.Entities;

public class EventSeries
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string RecurrencePattern { get; set; } = string.Empty; 

    public DateTime EndDate { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}