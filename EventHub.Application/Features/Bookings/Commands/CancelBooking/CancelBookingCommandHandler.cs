using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);

        if (booking == null)
        {
            throw new KeyNotFoundException($"Rezervacija sa ID-jem {request.BookingId} nije pronađena.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Samo rezervacije sa statusom 'Pending' mogu biti otkazane.");
        }

        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(booking.TicketTypeId);
        if (ticketType != null)
        {
            ticketType.ReleaseReservation(booking.Quantity);
            _unitOfWork.TicketTypes.Update(ticketType);
        }

        booking.Status = BookingStatus.Cancelled;

        await _unitOfWork.CompleteAsync();
    }
}