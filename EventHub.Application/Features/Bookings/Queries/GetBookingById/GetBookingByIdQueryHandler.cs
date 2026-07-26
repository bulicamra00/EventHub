using MediatR;
using EventHub.Domain.Interfaces;
using EventHub.Application.Features.Bookings.Queries.GetMyBookings;

namespace EventHub.Application.Features.Bookings.Queries.GetBookingById;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookingByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _unitOfWork.Bookings.GetListByConditionAsync(
            b => b.Id == request.Id, 
            "Event"
        );
        
        var booking = bookings.FirstOrDefault();
        
        if (booking == null) 
            return null;

        return new BookingDto 
        {
            Id = booking.Id,
            EventTitle = booking.Event?.Title ?? "Nepoznat događaj",
            TicketTypeId = booking.TicketTypeId, 
            Quantity = booking.Quantity,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt
        };
    }
}