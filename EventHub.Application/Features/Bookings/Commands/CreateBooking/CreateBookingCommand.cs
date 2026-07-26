using MediatR;

namespace EventHub.Application.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    Guid EventId,
    Guid TicketTypeId,
    int Quantity
) : IRequest<Guid>; 