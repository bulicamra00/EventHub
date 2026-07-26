using FluentValidation;

namespace EventHub.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID notifikacije je obavezan.");
    }
}