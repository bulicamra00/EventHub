using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketCommandHandler : IRequestHandler<PurchaseTicketCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJobService _jobService;

    public PurchaseTicketCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IJobService jobService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _jobService = jobService;
    }

    public async Task<Guid> Handle(PurchaseTicketCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId == null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Korisnik nije ulogovan.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null)
        {
            throw new KeyNotFoundException("Korisnik nije pronađen u sistemu.");
        }

        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(request.TicketTypeId);
        
        if (ticketType == null)
        {
            throw new KeyNotFoundException("Tip karte nije pronađen.");
        }

        if (!ticketType.HasAvailableCapacity(request.Quantity))
        {
            throw new InvalidOperationException("Nažalost, nema dovoljno dostupnih karata za ovaj događaj.");
        }

        ticketType.ConfirmPurchase(request.Quantity);

        var ticket = new Ticket
        {
            TicketTypeId = ticketType.Id,
            EventId = ticketType.EventId,
            UserId = userId.Value,
            AttendeeName = user.FullName,  
            AttendeeEmail = user.Email,    
            PurchasePrice = ticketType.GetCurrentPrice(),
            Status = TicketStatus.PendingPayment, 
            TicketCode = Guid.NewGuid().ToString() 
        };

        await _unitOfWork.Tickets.AddAsync(ticket);
        await _unitOfWork.CompleteAsync();
        
        _jobService.EnqueuePaymentProcessing(ticket.Id);

        return ticket.Id;
    }
}