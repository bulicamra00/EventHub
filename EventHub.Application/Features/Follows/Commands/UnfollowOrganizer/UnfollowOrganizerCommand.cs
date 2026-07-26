namespace EventHub.Application.Features.Follows.Commands.UnfollowOrganizer;
using MediatR;

public record UnfollowOrganizerCommand(Guid OrganizerId) : IRequest<bool>;