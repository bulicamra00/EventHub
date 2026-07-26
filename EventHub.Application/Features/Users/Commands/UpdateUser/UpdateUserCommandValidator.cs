using FluentValidation;

namespace EventHub.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ime i prezime su obavezni.")
            .MaximumLength(100).WithMessage("Ime ne može biti duže od 100 karaktera.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Grad je obavezan.")
            .MaximumLength(50).WithMessage("Naziv grada je predugačak.");
            
        RuleFor(x => x.Interests)
            .MaximumLength(500).WithMessage("Lista interesovanja je predugačka.");
    }
}