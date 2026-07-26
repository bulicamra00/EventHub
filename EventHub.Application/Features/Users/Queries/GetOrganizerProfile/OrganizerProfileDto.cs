using System;
using System.Collections.Generic;

namespace EventHub.Application.Features.Users.Queries.GetOrganizerProfile;

public record OrganizerProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string City,
    DateTime CreatedAt,
    int FollowersCount,
    List<OrganizerFollowerDto> Followers,
    List<OrganizedEventDto> CreatedEvents
);

public record OrganizerFollowerDto(
    Guid Id,
    string FullName,
    string Email
);

public record OrganizedEventDto(
    Guid Id,
    string Title,
    DateTime StartDate,
    string Status
);