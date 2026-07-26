using EventHub.Application.Features.Events.Queries.GetPersonalizedEvents;
using EventHub.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Events.Queries.GetPersonalizedEvents;

public class GetPersonalizedEventsQueryHandler : IRequestHandler<GetPersonalizedEventsQuery, List<EventSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPersonalizedEventsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<EventSummaryDto>> Handle(GetPersonalizedEventsQuery request, CancellationToken ct)
    {
        var followerId = _currentUserService.UserId;
        if (followerId == null) throw new UnauthorizedAccessException();

        var follows = await _unitOfWork.Follows.GetListByConditionAsync(f => f.FollowerId == followerId);
        var organizerIds = follows.Select(f => f.OrganizerId).ToList();

        if (!organizerIds.Any()) return new List<EventSummaryDto>();

        var events = await _unitOfWork.Events.GetListByConditionAsync(
            e => organizerIds.Contains(e.OrganizerId) && !e.IsPrivate, 
            "Organizer", 
            "EventTags.Tag"
        );

        return events.Select(e => new EventSummaryDto(
            e.Id,
            e.Title,
            e.StartDate,
            e.Location,
            e.CoverImageUrl ?? string.Empty,
            e.Organizer != null ? e.Organizer.FullName : "Nepoznat organizator",
            e.EventTags != null ? e.EventTags.Select(et => et.Tag.Name).ToList() : new List<string>(),
            e.IsPrivate
        )).ToList();
    }
}