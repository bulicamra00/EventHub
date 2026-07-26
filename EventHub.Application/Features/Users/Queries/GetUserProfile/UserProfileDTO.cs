namespace EventHub.Application.Features.Users.Queries.GetUserProfile;

public record AttendedEventDto(Guid EventId, string Title, DateTime Date);

public record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string City,
    DateTime JoinedAt,
    List<string> Interests,
    int TotalBookingsCount,
    List<AttendedEventDto> AttendedEvents
);