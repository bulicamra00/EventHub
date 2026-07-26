using EventHub.Application.Features.Events.Queries.GetEvents; // Dodaj ovo!

namespace EventHub.Application.Features.Follows.Queries.GetOrganizerDetails;

public record OrganizerDetailsDto(
    Guid Id,
    string FullName,
    string Email,
    int PublishedEventsCount,
    bool IsFollowed,
    List<EventDto> Events
);