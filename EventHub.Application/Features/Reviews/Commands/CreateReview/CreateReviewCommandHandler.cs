using EventHub.Application.Features.Reviews.Commands.CreateReview;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("Korisnik nije ulogovan.");
        }

        var tickets = await _unitOfWork.Tickets.GetListByConditionAsync(
            t => t.EventId == request.EventId && 
                 t.UserId == currentUserId.Value && 
                 t.Status == TicketStatus.Used
        );

        if (!tickets.Any())
        {
            throw new Exception("Možete oceniti samo događaj kojem ste prisustvovali (karta mora biti skenirana).");
        }

        var existingReviews = await _unitOfWork.Reviews.GetListByConditionAsync(
            r => r.EventId == request.EventId && 
                 r.UserId == currentUserId.Value
        );

        if (existingReviews.Any())
        {
            throw new Exception("Već ste ocenili ovaj događaj.");
        }

        var review = new Review
        {
            EventId = request.EventId,
            UserId = currentUserId.Value, 
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.CompleteAsync();

        return review.Id;
    }
}