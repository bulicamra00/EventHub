using FluentValidation;

namespace EventHub.Application.Features.Reminders.Commands.SendReminders;

public class SendRemindersCommandValidator : AbstractValidator<SendRemindersCommand>
{
    public SendRemindersCommandValidator()
    {
    }
}