using FluentValidation;

namespace EventHub.Application.Features.Users.Commands.RevokeToken;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token za opoziv je obavezan.")
            .NotNull().WithMessage("Token ne sme biti null.");
    }
}