using MediatR;

namespace EventHub.Application.Features.Follows.Queries.GetFollowedOrganizers;

public record GetFollowedOrganizersQuery : IRequest<List<FollowedOrganizerDto>>;