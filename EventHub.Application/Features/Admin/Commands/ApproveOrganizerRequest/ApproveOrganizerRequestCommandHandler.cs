using MediatR;
using EventHub.Domain.Interfaces;
using EventHub.Domain.Enums; 
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EventHub.Application.Features.Admin.Commands.ApproveOrganizerRequest;

public class ApproveOrganizerRequestCommandHandler : IRequestHandler<ApproveOrganizerRequestCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveOrganizerRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ApproveOrganizerRequestCommand request, CancellationToken cancellationToken)
    {
        
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"Korisnik sa ID-jem {request.UserId} nije pronađen.");
        }

        
        user.IsOrganizerRequested = false; 
        user.OrganizerRequestStatus = "Approved";
        user.Role = UserRole.Organizer; 

        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}