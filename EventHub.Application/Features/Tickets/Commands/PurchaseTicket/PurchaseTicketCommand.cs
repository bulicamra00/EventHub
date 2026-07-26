using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.PurchaseTicket;

public record PurchaseTicketCommand(
    Guid TicketTypeId, 
    int Quantity, 
    string AttendeeName, 
    string AttendeeEmail
) : IRequest<Guid>; 