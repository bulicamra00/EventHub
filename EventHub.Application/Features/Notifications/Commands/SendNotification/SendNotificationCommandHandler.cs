using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public SendNotificationCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (eventEntity == null) throw new Exception("Događaj nije pronađen.");

        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(
            t => t.EventId == request.EventId && t.Status != TicketStatus.Cancelled, "User");

        foreach (var ticket in tickets)
        {
            if (ticket.User == null || string.IsNullOrEmpty(ticket.User.Email)) continue;

            var emailBody = $"Poruka od organizatora događaja '{eventEntity.Title}':\n\n{request.Message}";
            await _emailService.SendEmailAsync(ticket.User.Email, request.Subject, emailBody);

            var notification = new Notification
            {
                UserId = ticket.UserId,
                Message = $"[Obaveštenje za {eventEntity.Title}]: {request.Subject} - {request.Message}"
            };
            
            await _unitOfWork.Notifications.AddAsync(notification);
        }

        return await _unitOfWork.CompleteAsync() > 0;
    }
}