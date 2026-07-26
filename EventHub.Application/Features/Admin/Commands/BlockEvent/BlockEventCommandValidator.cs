using FluentValidation;

namespace EventHub.Application.Features.Admin.Commands.BlockEvent;

public class BlockEventCommandValidator : AbstractValidator<BlockEventCommand>
{
    public BlockEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("ID događaja je obavezan.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Razlog blokiranja je obavezan.")
            .MaximumLength(500).WithMessage("Razlog ne može biti duži od 500 karaktera.");
    }
}