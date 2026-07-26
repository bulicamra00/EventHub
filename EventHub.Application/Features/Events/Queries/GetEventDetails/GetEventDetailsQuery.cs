using MediatR;

namespace EventHub.Application.Features.Events.Queries.GetEventDetails;

public record GetEventDetailsQuery(Guid Id, string? Token) : IRequest<EventDetailsDto>;