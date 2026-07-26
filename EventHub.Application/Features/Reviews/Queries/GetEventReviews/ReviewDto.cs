namespace EventHub.Application.Features.Reviews.Queries.GetEventReviews;

public record ReviewDto(
    Guid Id,
    string UserName,
    int Rating,
    string Comment,
    DateTime CreatedAt
);