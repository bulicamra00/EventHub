using EventHub.Application.Features.Reviews.Queries.GetEventReviews;
using EventHub.Domain.Interfaces;
using MediatR;
using AutoMapper;

namespace EventHub.Application.Features.Reviews.Queries.GetEventReviews;

public class GetEventReviewsQueryHandler : IRequestHandler<GetEventReviewsQuery, List<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEventReviewsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(GetEventReviewsQuery request, CancellationToken ct)
    {
        var reviews = await _unitOfWork.Reviews.GetListByConditionAsync(
            r => r.EventId == request.EventId, 
            "User" 
        );

        return reviews
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(
                r.Id,
                r.User != null ? r.User.FullName : "Anonimni korisnik",
                r.Rating,
                r.Comment,
                r.CreatedAt
            ))
            .ToList();
    }
}