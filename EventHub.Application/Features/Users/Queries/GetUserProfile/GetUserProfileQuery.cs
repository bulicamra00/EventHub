using MediatR;

namespace EventHub.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery : IRequest<UserProfileDto>;