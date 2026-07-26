using MediatR;
using EventHub.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventHub.Application.Features.Users.Commands.RequestOrganizer;

public class RequestOrganizerCommandHandler : IRequestHandler<RequestOrganizerCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RequestOrganizerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RequestOrganizerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);

        if (user == null)
            throw new Exception("Korisnik nije pronađen.");

        user.IsOrganizerRequested = true;
        user.OrganizerRequestStatus = "Pending";

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}