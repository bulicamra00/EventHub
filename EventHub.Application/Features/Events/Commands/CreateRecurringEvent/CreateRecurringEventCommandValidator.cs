using FluentValidation;

namespace EventHub.Application.Features.Events.Commands.CreateRecurringEvent;

public class CreateRecurringEventCommandValidator : AbstractValidator<CreateRecurringEventCommand>
{
    public CreateRecurringEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naslov je obavezan.")
            .MaximumLength(200).WithMessage("Naslov ne sme biti duži od 200 karaktera.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Datum početka je obavezan.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Datum ne može biti u prošlosti.");

        RuleFor(x => x.NumberOfWeeks)
            .InclusiveBetween(1, 52).WithMessage("Broj nedelja mora biti između 1 i 52.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategorija je obavezna.");
    }
}