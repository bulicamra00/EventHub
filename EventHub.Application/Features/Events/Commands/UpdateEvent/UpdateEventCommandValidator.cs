using FluentValidation;

namespace EventHub.Application.Features.Events.Commands.UpdateEvent;

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID događaja je obavezan.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naziv događaja je obavezan.")
            .MaximumLength(150).WithMessage("Naziv ne može imati više od 150 karaktera.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Opis događaja je obavezan.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Datum početka je obavezan.")
            .GreaterThan(DateTime.Now).WithMessage("Datum početka mora biti u budućnosti.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("Datum završetka mora biti nakon datuma početka.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategorija događaja je obavezna.");
    }
}