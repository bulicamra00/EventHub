using MediatR;

namespace EventHub.Application.Features.Users.Queries.GetOrganizerProfile;

public record GetOrganizerProfileQuery : IRequest<OrganizerProfileDto>;