using FluentValidation;

namespace EventHub.Application.Features.Events.Commands.PublishEvent;

public class PublishEventCommandValidator : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(v => v.EventId)
            .NotEmpty().WithMessage("Event ID je obavezan.");
    }
}