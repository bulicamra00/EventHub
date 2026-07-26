namespace EventHub.Application.Features.Events.Queries.GetPersonalizedEvents;

public record EventSummaryDto(
    Guid Id, 
    string Title, 
    DateTime StartDate, 
    string Location, 
    string CoverImageUrl,      
    string OrganizerName,      
    List<string> TagNames,     
    bool IsPrivate    
);