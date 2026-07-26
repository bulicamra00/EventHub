using EventHub.Application.Exceptions;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Admin.Commands.UnblockUser;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnblockUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

        if (user == null)
            throw new NotFoundException(nameof(User), request.UserId);

        user.IsBlocked = false;
        user.BanReason = null; 

        await _unitOfWork.CompleteAsync();
    }
}