using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.CancelTicket;

public record CancelTicketCommand(Guid TicketId) : IRequest<bool>;