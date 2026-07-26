using FluentValidation;

namespace EventHub.Application.Features.Events.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token pozivnice je obavezan.")
            .MinimumLength(10).WithMessage("Token nije validnog formata.");
    }
}