using EventHub.Application.Features.Follows.Queries.GetFollowedOrganizers;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Follows.Queries.GetFollowedOrganizers;

public class GetFollowedOrganizersQueryHandler : IRequestHandler<GetFollowedOrganizersQuery, List<FollowedOrganizerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetFollowedOrganizersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<FollowedOrganizerDto>> Handle(GetFollowedOrganizersQuery request, CancellationToken ct)
    {
        var followerId = _currentUserService.UserId;
        if (followerId == null) throw new UnauthorizedAccessException();

        var followed = await _unitOfWork.Follows.GetListByConditionAsync(f => f.FollowerId == followerId);
        
        var organizerIds = followed.Select(f => f.OrganizerId).ToList();

        var organizers = await _unitOfWork.Users.GetListByConditionAsync(u => organizerIds.Contains(u.Id));

        return organizers.Select(u => new FollowedOrganizerDto(u.Id, u.FullName, u.Email)).ToList();
    }
}