using FluentValidation;

namespace EventHub.Application.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketCommandValidator : AbstractValidator<PurchaseTicketCommand>
{
    public PurchaseTicketCommandValidator()
    {
        RuleFor(c => c.TicketTypeId)
            .NotEmpty().WithMessage("ID tipa karte je obavezan.");

        RuleFor(c => c.Quantity)
            .GreaterThan(0).WithMessage("Broj karata mora biti najmanje 1.")
            .LessThanOrEqualTo(10).WithMessage("Možete kupiti maksimalno 10 karata odjednom.");

        RuleFor(c => c.AttendeeName)
            .NotEmpty().WithMessage("Ime učesnika je obavezno.")
            .MaximumLength(100);

        RuleFor(c => c.AttendeeEmail)
            .NotEmpty().WithMessage("Email je obavezan.")
            .EmailAddress().WithMessage("Format email adrese nije ispravan.");
    }
}