using EventHub.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Application.Features.Follows.Commands.UnfollowOrganizer;

public class UnfollowOrganizerCommandHandler : IRequestHandler<UnfollowOrganizerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowOrganizerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UnfollowOrganizerCommand request, CancellationToken ct)
    {
        var followerId = _currentUserService.UserId;
        if (followerId == null) return false;

        var follows = await _unitOfWork.Follows.GetListByConditionAsync(f => 
            f.FollowerId == followerId && f.OrganizerId == request.OrganizerId);
        
        var follow = follows.FirstOrDefault();
        if (follow == null) return false;

        _unitOfWork.Follows.Delete(follow);
        
        await _unitOfWork.CompleteAsync();
        
        return true;
    }
}