using FluentValidation;

namespace EventHub.Application.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Refresh token je obavezan.")
            .NotNull().WithMessage("Refresh token ne sme biti null.");
    }
}