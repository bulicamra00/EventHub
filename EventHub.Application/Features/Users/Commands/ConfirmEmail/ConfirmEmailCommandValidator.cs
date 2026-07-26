using FluentValidation;

namespace EventHub.Application.Features.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token je obavezan.")
            .NotNull().WithMessage("Token ne sme biti null.");
    }
}