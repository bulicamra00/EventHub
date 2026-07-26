using FluentValidation;

namespace EventHub.Application.Features.Tickets.Commands.CancelTicket;

public class CancelTicketCommandValidator : AbstractValidator<CancelTicketCommand>
{
    public CancelTicketCommandValidator()
    {
        RuleFor(c => c.TicketId)
            .NotEmpty().WithMessage("ID karte je obavezan.");
    }
}