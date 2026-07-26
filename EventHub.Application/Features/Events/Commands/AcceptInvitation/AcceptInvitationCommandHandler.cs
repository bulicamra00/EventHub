using MediatR;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EventHub.Application.Features.Events.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AcceptInvitationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(AcceptInvitationCommand request, CancellationToken ct)
    {
        var invitation = await _unitOfWork.EventInvitations.GetByConditionAsync(x => x.Token == request.Token);
        
        if (invitation == null || invitation.IsUsed)
            return false;

        if (string.IsNullOrEmpty(_currentUserService.Email) || invitation.Email != _currentUserService.Email)
            return false;

        var userId = _currentUserService.UserId ?? throw new Exception("User ID nije pronađen.");
        var existingTickets = await _unitOfWork.Tickets.GetListByConditionAsync(t => 
            t.EventId == invitation.EventId && t.UserId == userId);
            
        if (existingTickets.Any())
            return false; 

        var ticketTypes = await _unitOfWork.TicketTypes.GetListByConditionAsync(t => t.EventId == invitation.EventId);
        var ticketType = ticketTypes.FirstOrDefault();

        if (ticketType == null)
            return false; 

        invitation.Accept();

        var ticket = new Ticket
        {
            EventId = invitation.EventId,
            UserId = userId,
            TicketTypeId = ticketType.Id,
            PurchaseDate = DateTime.UtcNow,
            Status = TicketStatus.Active,
            AttendeeEmail = invitation.Email
        };

        await _unitOfWork.Tickets.AddAsync(ticket);
        _unitOfWork.EventInvitations.Update(invitation);
        
        await _unitOfWork.CompleteAsync();
        
        return true;
    }
}