using MediatR;

namespace EventHub.Application.Features.Tickets.Queries.GetAttendees;

public record GetAttendeesQuery(Guid EventId) : IRequest<List<AttendeeDto>>;