using EventHub.Application.Features.Follows.Queries.GetOrganizers;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Follows.Queries.GetOrganizers;

public class GetOrganizersQueryHandler : IRequestHandler<GetOrganizersQuery, List<OrganizerSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOrganizersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<OrganizerSummaryDto>> Handle(GetOrganizersQuery request, CancellationToken ct)
    {
        var allOrganizers = await _unitOfWork.Users.GetListByConditionAsync(u => u.Role == UserRole.Organizer);

        var allEvents = await _unitOfWork.Events.GetListByConditionAsync(e => !e.IsPrivate);

        var followedIds = new List<Guid>();
        var currentUserId = _currentUserService.UserId;
        
        if (currentUserId != null)
        {
            var follows = await _unitOfWork.Follows.GetListByConditionAsync(f => f.FollowerId == currentUserId);
            followedIds = follows.Select(f => f.OrganizerId).ToList();
        }

        return allOrganizers.Select(o => new OrganizerSummaryDto(
            o.Id,
            o.FullName,
            followedIds.Contains(o.Id),
            allEvents.Count(e => e.OrganizerId == o.Id) 
        )).ToList();
    }
}