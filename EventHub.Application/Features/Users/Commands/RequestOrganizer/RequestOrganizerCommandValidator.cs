using FluentValidation;

namespace EventHub.Application.Features.Users.Commands.RequestOrganizer;

public class RequestOrganizerCommandValidator : AbstractValidator<RequestOrganizerCommand>
{
    public RequestOrganizerCommandValidator()
    {
    }
}