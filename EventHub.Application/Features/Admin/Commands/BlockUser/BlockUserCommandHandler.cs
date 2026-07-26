using EventHub.Application.Exceptions;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Admin.Commands.BlockUser;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public BlockUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

        if (user == null)
            throw new NotFoundException(nameof(User), request.UserId);

        user.IsBlocked = true;
        user.BanReason = request.Reason;

        await _unitOfWork.CompleteAsync();
    }
}