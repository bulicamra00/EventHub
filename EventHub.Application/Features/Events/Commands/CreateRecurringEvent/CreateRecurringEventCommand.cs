using MediatR;

namespace EventHub.Application.Features.Events.Commands.CreateRecurringEvent;

public class CreateRecurringEventCommand : IRequest<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int NumberOfWeeks { get; set; }
    
    public Guid CategoryId { get; set; } 
    
    public string Location { get; set; } = string.Empty;
}