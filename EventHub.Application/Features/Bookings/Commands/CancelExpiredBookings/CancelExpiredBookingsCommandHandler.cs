using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;

public class CancelExpiredBookingsCommandHandler : IRequestHandler<CancelExpiredBookingsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelExpiredBookingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelExpiredBookingsCommand request, CancellationToken ct)
    {
        var expiredBookings = await _unitOfWork.Bookings.GetListByConditionAsync(
            b => b.Status == BookingStatus.Pending && b.ExpiresAt < DateTime.UtcNow);

        foreach (var booking in expiredBookings)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(booking.TicketTypeId);
            if (ticketType != null)
            {
                ticketType.ReleaseReservation(booking.Quantity);
                
                _unitOfWork.TicketTypes.Update(ticketType);
            }
            
            booking.Status = BookingStatus.Expired;
        }

        if (expiredBookings.Any())
        {
            await _unitOfWork.CompleteAsync();
        }
    }
}