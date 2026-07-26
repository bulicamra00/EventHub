using MediatR;

namespace EventHub.Application.Features.Events.Commands.CreateInvitation;

public record CreateInvitationCommand(Guid EventId, string Email) : IRequest<Guid>;