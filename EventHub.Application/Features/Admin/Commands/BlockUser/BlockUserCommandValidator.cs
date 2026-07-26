using FluentValidation;

namespace EventHub.Application.Features.Admin.Commands.BlockUser;

public class BlockUserCommandValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Korisnički ID je obavezan.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Razlog blokiranja je obavezan.")
            .MinimumLength(5).WithMessage("Razlog mora imati najmanje 5 karaktera.")
            .MaximumLength(500).WithMessage("Razlog ne može biti duži od 500 karaktera.");
    }
}