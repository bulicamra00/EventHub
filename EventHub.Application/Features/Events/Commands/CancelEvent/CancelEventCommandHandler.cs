using EventHub.Application.Features.Events.Commands.CancelEvent;
using EventHub.Domain.Entities; 
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Events.Commands.CancelEvent;

public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public CancelEventCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<bool> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (eventEntity == null) throw new Exception("Događaj nije pronađen.");

        eventEntity.Cancel(request.Reason);

        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(t => 
            t.EventId == request.EventId && t.Status != TicketStatus.Cancelled, "User");

        foreach (var ticket in tickets)
        {
            var emailBody = $"Poštovani, događaj '{eventEntity.Title}' je otkazan. Razlog: {request.Reason}";
            await _emailService.SendEmailAsync(ticket.User.Email, "Obaveštenje o otkazivanju događaja", emailBody);

            var notification = new Notification
            {
                UserId = ticket.UserId,
                Message = $"Događaj '{eventEntity.Title}' je otkazan. Razlog: {request.Reason}"
            };
            
            await _unitOfWork.Notifications.AddAsync(notification);
        }

        _unitOfWork.Events.Update(eventEntity);
        return await _unitOfWork.CompleteAsync() > 0;
    }
}