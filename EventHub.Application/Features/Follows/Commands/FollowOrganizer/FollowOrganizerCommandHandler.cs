using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Follows.Commands.FollowOrganizer;

public class FollowOrganizerCommandHandler : IRequestHandler<FollowOrganizerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FollowOrganizerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(FollowOrganizerCommand request, CancellationToken ct)
    {
        var followerId = _currentUserService.UserId;
        if (followerId == null) 
            throw new UnauthorizedAccessException("Morate biti ulogovani da biste pratili organizatora.");

        var existingFollow = await _unitOfWork.Follows.GetByConditionAsync(f => 
            f.FollowerId == followerId && f.OrganizerId == request.OrganizerId);
        
        if (existingFollow != null) return true; 

        var follow = new Follow
        {
            FollowerId = followerId.Value,
            OrganizerId = request.OrganizerId
        };

        await _unitOfWork.Follows.AddAsync(follow);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}