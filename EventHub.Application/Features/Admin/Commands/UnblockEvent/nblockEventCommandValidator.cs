using FluentValidation;

namespace EventHub.Application.Features.Admin.Commands.UnblockEvent;

public class UnblockEventCommandValidator : AbstractValidator<UnblockEventCommand>
{
    public UnblockEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("ID događaja je obavezan.");
    }
}