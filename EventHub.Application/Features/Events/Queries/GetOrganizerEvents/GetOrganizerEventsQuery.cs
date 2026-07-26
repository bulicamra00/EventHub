using MediatR;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Domain.Enums; 

namespace EventHub.Application.Features.Events.Queries.GetOrganizerEvents;

public record GetOrganizerEventsQuery(
    int PageNumber = 1, 
    int PageSize = 10, 
    EventStatus? Status = null) 
    : IRequest<OrganizerEventsResponse>;

public record OrganizerEventsResponse(IEnumerable<EventDto> Items, int TotalCount);