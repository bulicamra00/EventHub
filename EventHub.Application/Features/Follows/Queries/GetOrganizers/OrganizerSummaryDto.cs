namespace EventHub.Application.Features.Follows.Queries.GetOrganizers;

public record OrganizerSummaryDto(
    Guid Id, 
    string FullName, 
    bool IsFollowed,
    int PublishedEventsCount 
);