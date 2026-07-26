using FluentValidation;

namespace EventHub.Application.Features.Tickets.Commands.CreateTicketType;

public class CreateTicketTypeCommandValidator : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Naziv tipa karte je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne sme biti duži od 100 karaktera.");

        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Cena ne može biti negativna.");

        RuleFor(c => c.Capacity)
            .GreaterThan(0).WithMessage("Kapacitet mora biti najmanje 1.");

        RuleFor(c => c.EarlyBirdPrice)
            .LessThanOrEqualTo(c => c.Price)
            .When(c => c.EarlyBirdPrice.HasValue && c.EarlyBirdPrice > 0)
            .WithMessage("Early-bird cena mora biti manja ili jednaka osnovnoj ceni.");
    }
}