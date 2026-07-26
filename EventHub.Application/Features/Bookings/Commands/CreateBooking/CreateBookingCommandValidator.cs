using FluentValidation;

namespace EventHub.Application.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Događaj je obavezan.");

        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Tip karte je obavezan.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Broj karata mora biti veći od 0.")
            .LessThanOrEqualTo(10).WithMessage("Ne možete rezervisati više od 10 karata odjednom.");
    }
}