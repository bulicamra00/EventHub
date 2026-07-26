using EventHub.Application.Features.Follows.Queries.GetOrganizerDetails;
using EventHub.Application.Features.Events.Queries.GetEvents;
using EventHub.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventHub.Application.Features.Follows.Queries.GetOrganizerDetails;

public class GetOrganizerDetailsQueryHandler : IRequestHandler<GetOrganizerDetailsQuery, OrganizerDetailsDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOrganizerDetailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<OrganizerDetailsDto?> Handle(GetOrganizerDetailsQuery request, CancellationToken ct)
    {
        var organizer = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (organizer == null) return null;

        var events = await _unitOfWork.Events.GetListByConditionAsync(e => e.OrganizerId == request.Id && !e.IsPrivate);

        bool isFollowed = false;
        var currentUserId = _currentUserService.UserId;
        if (currentUserId != null)
        {
            var follows = await _unitOfWork.Follows.GetListByConditionAsync(f => f.FollowerId == currentUserId && f.OrganizerId == request.Id);
            isFollowed = follows.Any();
        }

        return new OrganizerDetailsDto(
            organizer.Id,
            organizer.FullName,
            organizer.Email ?? string.Empty,
            events.Count(),
            isFollowed,
            events.Select(e => new EventDto {
                Id = e.Id,
                Title = e.Title ?? string.Empty,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location ?? string.Empty,
                CoverImageUrl = e.CoverImageUrl ?? string.Empty,
                Status = e.Status
            }).ToList()
        );
    }
}