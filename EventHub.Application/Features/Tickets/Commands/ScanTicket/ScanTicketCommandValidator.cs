using FluentValidation;

namespace EventHub.Application.Features.Tickets.Commands.ScanTicket;

public class ScanTicketCommandValidator : AbstractValidator<ScanTicketCommand>
{
    public ScanTicketCommandValidator()
    {
        RuleFor(x => x.TicketCode)
            .NotEmpty().WithMessage("Kod karte je obavezan.")
            .NotNull().WithMessage("Kod karte ne sme biti prazan.");

        RuleFor(x => x.TicketCode)
            .Length(36).WithMessage("Kod karte nije validnog formata.");

        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("ID događaja je obavezan.");
    }
}