using FluentValidation;

namespace EventHub.Application.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("ID događaja je obavezan.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Naslov obaveštenja je obavezan.")
            .MaximumLength(150).WithMessage("Naslov ne može imati više od 150 karaktera.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Tekst obaveštenja je obavezan.");
    }
}