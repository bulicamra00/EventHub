using EventHub.Domain.Interfaces;
using EventHub.Domain.Entities;
using MediatR;

namespace EventHub.Application.Features.Reminders.Commands.SendReminders;

public class SendRemindersCommandHandler : IRequestHandler<SendRemindersCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public SendRemindersCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task Handle(SendRemindersCommand request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        
        var targetTime = now.AddHours(24);
        var windowStart = targetTime.AddMinutes(-5);
        var windowEnd = targetTime.AddMinutes(5);

        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(t => 
            !t.ReminderSent && 
            t.Event.StartDate >= windowStart && 
            t.Event.StartDate <= windowEnd, 
            "Event"); 

        foreach (var ticket in tickets)
        {
            if (ticket.Event == null) continue;

            try 
            {
                await _emailService.SendEmailAsync(ticket.AttendeeEmail, 
                    $"Podsetnik: {ticket.Event.Title} počinje za 24h!", 
                    $"Pozdrav {ticket.AttendeeName}, podsećamo vas da vaš događaj počinje sutra u ovo vreme.");

                ticket.ReminderSent = true;
                ticket.ReminderSentAt = DateTime.UtcNow;
            }
            catch (Exception)
            {
            }
        }

        if (tickets.Any())
        {
            await _unitOfWork.CompleteAsync();
        }
    }
}