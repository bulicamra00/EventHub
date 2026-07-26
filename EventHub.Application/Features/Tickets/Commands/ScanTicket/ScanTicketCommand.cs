using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.ScanTicket;

public record ScanTicketCommand(string TicketCode, Guid EventId) : IRequest<bool>;