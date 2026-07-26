using EventHub.Application.Features.Tickets.Commands.CancelTicket;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.CancelTicket;

public class CancelTicketCommandHandler : IRequestHandler<CancelTicketCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelTicketCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(CancelTicketCommand request, CancellationToken cancellationToken)
    {
        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(t => t.Id == request.TicketId, "Event");
        var ticket = tickets.FirstOrDefault();

        if (ticket == null) return false;

        if (ticket.UserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("Nemate dozvolu da otkažete ovu kartu.");

        if (ticket.Status == TicketStatus.Cancelled || ticket.Status == TicketStatus.Used)
            return false;

        if (ticket.Event != null && DateTime.UtcNow.AddHours(24) > ticket.Event.StartDate)
        {
            throw new Exception("Otkazivanje nije moguće manje od 24 sata pre početka događaja.");
        }

        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticket.TicketTypeId);
        if (ticketType != null)
        {
            ticketType.CancelSoldTicket(1);
            _unitOfWork.TicketTypes.Update(ticketType);
        }

        ticket.Status = TicketStatus.Cancelled;
        _unitOfWork.Tickets.Update(ticket);

        if (ticket.Event != null && ticket.Event.Status == EventStatus.SoldOut)
        {
            if (ticketType != null && ticketType.SoldCount < ticketType.Capacity)
            {
                ticket.Event.Publish(); 
                _unitOfWork.Events.Update(ticket.Event);
            }
        }

        var result = await _unitOfWork.CompleteAsync();

        return result > 0;
    }
}