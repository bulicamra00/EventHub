using MediatR;

namespace EventHub.Application.Features.Tickets.Queries.GetMyTickets;

public record GetMyTicketsQuery() : IRequest<List<TicketDto>>;