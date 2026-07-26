using EventHub.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventHub.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("Korisnik nije ulogovan.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Korisnik nije pronađen.");
        }

        user.FullName = request.FullName;
        user.City = request.City;
        user.Interests = request.Interests;

        _unitOfWork.Users.Update(user); 
        
        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}