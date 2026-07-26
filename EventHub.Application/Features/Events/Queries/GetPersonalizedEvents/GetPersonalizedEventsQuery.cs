using MediatR;

namespace EventHub.Application.Features.Events.Queries.GetPersonalizedEvents;

public record GetPersonalizedEventsQuery : IRequest<List<EventSummaryDto>>;