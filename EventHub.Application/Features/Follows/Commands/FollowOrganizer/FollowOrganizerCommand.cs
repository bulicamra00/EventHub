using MediatR;

namespace EventHub.Application.Features.Follows.Commands.FollowOrganizer;

public record FollowOrganizerCommand(Guid OrganizerId) : IRequest<bool>;