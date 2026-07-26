using MediatR;

namespace EventHub.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommand(Guid EventId, int Rating, string Comment) : IRequest<Guid>;