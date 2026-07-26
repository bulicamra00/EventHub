using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Tickets.Commands.ScanTicket;

public class ScanTicketCommandHandler : IRequestHandler<ScanTicketCommand, bool>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScanTicketCommandHandler(IGenericRepository<Ticket> ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ScanTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByConditionAsync(
            t => t.TicketCode == request.TicketCode && t.EventId == request.EventId
        );

        if (ticket == null)
        {
            return false;
        }

        if (ticket.Status == TicketStatus.Used || ticket.Status == TicketStatus.Cancelled)
        {
            return false;
        }

        ticket.Status = TicketStatus.Used;

        _ticketRepository.Update(ticket);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0;
    }
}