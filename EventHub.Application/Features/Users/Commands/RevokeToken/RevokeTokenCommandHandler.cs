using EventHub.Application.Features.Users.Commands.RevokeToken;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Users.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByConditionAsync(u => u.RefreshToken == request.Token);

        if (user == null)
            throw new KeyNotFoundException("Token nije pronađen.");

        if (user.RefreshTokenRevoked != null)
            throw new UnauthorizedAccessException("Token je već opozvan.");

        user.RefreshTokenRevoked = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();
    }
}