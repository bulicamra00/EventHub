using FluentValidation;

namespace EventHub.Application.Features.Follows.Commands.FollowOrganizer;

public class FollowOrganizerCommandValidator : AbstractValidator<FollowOrganizerCommand>
{
    public FollowOrganizerCommandValidator()
    {
        RuleFor(x => x.OrganizerId)
            .NotEmpty().WithMessage("ID organizatora je obavezan.");
    }
}