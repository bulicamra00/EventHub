using EventHub.Application.Features.Users.Queries.GetOrganizerProfile;
using EventHub.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Users.Queries.GetOrganizerProfile;

public class GetOrganizerProfileQueryHandler : IRequestHandler<GetOrganizerProfileQuery, OrganizerProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOrganizerProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<OrganizerProfileDto> Handle(GetOrganizerProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Korisnik nije ulogovan.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception($"Korisnik sa ID-em {userId} nije pronađen.");
        }

        var organizedEvents = await _unitOfWork.Events.GetListByConditionAsync(e => e.OrganizerId == userId);

        var eventDtos = organizedEvents.Select(e => new OrganizedEventDto(
            e.Id,
            e.Title,
            e.StartDate,
            e.Status.ToString()
        )).ToList();

        var followsList = await _unitOfWork.Follows.GetListByConditionAsync(f => f.OrganizerId == userId, "Follower");

        var followersList = followsList.Select(f => new OrganizerFollowerDto(
            f.Follower.Id,
            f.Follower.FullName,
            f.Follower.Email
        )).ToList();

        var dto = new OrganizerProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.City ?? string.Empty,
            user.CreatedAt,
            followersList.Count,
            followersList,
            eventDtos
        );

        return dto;
    }
}