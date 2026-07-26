using MediatR;

namespace EventHub.Application.Features.Reviews.Queries.GetEventReviews;

public record GetEventReviewsQuery(Guid EventId) : IRequest<List<ReviewDto>>;