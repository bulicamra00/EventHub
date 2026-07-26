using FluentValidation;

namespace EventHub.Application.Features.Admin.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ime kategorije je obavezno.")
            .MaximumLength(100).WithMessage("Ime ne može biti duže od 100 karaktera.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Opis ne može biti duži od 500 karaktera.");
    }
}