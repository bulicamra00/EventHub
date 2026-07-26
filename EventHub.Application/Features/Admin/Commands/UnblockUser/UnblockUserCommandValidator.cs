using FluentValidation;

namespace EventHub.Application.Features.Admin.Commands.UnblockUser;

public class UnblockUserCommandValidator : AbstractValidator<UnblockUserCommand>
{
    public UnblockUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Korisnički ID je obavezan.");
    }
}