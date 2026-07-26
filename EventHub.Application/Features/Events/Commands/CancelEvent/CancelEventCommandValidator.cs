using FluentValidation;

namespace EventHub.Application.Features.Events.Commands.CancelEvent;

public class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("ID događaja je obavezan.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Razlog otkazivanja je obavezan.")
            .MaximumLength(500).WithMessage("Razlog ne može biti duži od 500 karaktera.");
    }
}