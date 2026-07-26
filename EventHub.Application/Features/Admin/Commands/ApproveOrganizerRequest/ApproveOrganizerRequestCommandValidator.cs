using FluentValidation;
using EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;
using System;

namespace EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;

public class ApproveOrganizerRequestCommandValidator : AbstractValidator<ApproveOrganizerRequestCommand>
{
    public ApproveOrganizerRequestCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID korisnika je obavezan.")
            .NotEqual(Guid.Empty).WithMessage("ID korisnika ne može biti prazan (Guid.Empty).");
    }
}