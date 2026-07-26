using MediatR;

namespace EventHub.Application.Features.Bookings.Commands.CancelBooking;

public class CancelBookingCommand : IRequest
{
    public Guid BookingId { get; set; }
}