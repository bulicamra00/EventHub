using FluentValidation;

namespace EventHub.Application.Features.Follows.Commands.UnfollowOrganizer;

public class UnfollowOrganizerCommandValidator : AbstractValidator<UnfollowOrganizerCommand>
{
    public UnfollowOrganizerCommandValidator()
    {
        RuleFor(x => x.OrganizerId)
            .NotEmpty().WithMessage("ID organizatora je obavezan.");
    }
}