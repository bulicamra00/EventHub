using FluentValidation;

namespace EventHub.Application.Features.Events.Commands.CreateEvent;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.StartDate).NotEmpty().GreaterThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
            .WithMessage("Datum kraja mora biti nakon datuma početka.");
        RuleFor(x => x.CategoryId).NotEmpty();
        
        RuleFor(x => x.TicketTypes)
            .NotEmpty().WithMessage("Događaj mora imati bar jedan tip karte.");
        
        RuleForEach(x => x.TicketTypes).ChildRules(ticket => {
            ticket.RuleFor(t => t.Name).NotEmpty().MaximumLength(100);
            ticket.RuleFor(t => t.Price).GreaterThanOrEqualTo(0);
            ticket.RuleFor(t => t.Capacity).GreaterThan(0);
        });

        RuleForEach(x => x.TagNames).NotEmpty().MaximumLength(50);
    }
}