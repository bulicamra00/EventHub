using MediatR;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateBookingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedAccessException("Korisnik nije ulogovan.");

        var @event = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (@event == null)
            throw new Exception("Događaj nije pronađen.");

        
        if (@event.Status != EventStatus.Published)
            throw new InvalidOperationException($"Rezervacija nije moguća: Događaj je u statusu {@event.Status}.");

        if (@event.StartDate <= DateTime.UtcNow.AddHours(1))
        {
            @event.Complete();
            await _unitOfWork.CompleteAsync();
            
            throw new InvalidOperationException("Prodaja je zatvorena: Događaj počinje za manje od sat vremena ili je već prošao.");
        }


        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(request.TicketTypeId);
        if (ticketType == null)
            throw new Exception("Tip karte nije pronađen.");

        if (!ticketType.HasAvailableCapacity(request.Quantity))
            throw new InvalidOperationException("Nema dovoljno mesta za odabranu kartu.");

        var booking = new Booking
        {
            UserId = userId.Value,
            EventId = request.EventId,
            TicketTypeId = request.TicketTypeId,
            Quantity = request.Quantity,
            TotalPrice = ticketType.GetCurrentPrice() * request.Quantity,
            Status = BookingStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10) 
        };

        ticketType.Reserve(request.Quantity);

        var totalCapacity = @event.TicketTypes.Sum(t => t.Capacity);
        var currentReserved = @event.TicketTypes.Sum(t => t.ReservedCount);
        
        @event.UpdateStatusIfSoldOut(totalCapacity, currentReserved);


        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.CompleteAsync();

        return booking.Id;
    }
}