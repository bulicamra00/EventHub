namespace EventHub.Application.Features.Follows.Queries.GetFollowedOrganizers;

public record FollowedOrganizerDto(Guid Id, string FullName, string Email);