using MediatR;

namespace EventHub.Application.Features.Events.Commands.AcceptInvitation;

public record AcceptInvitationCommand(string Token) : IRequest<bool>;