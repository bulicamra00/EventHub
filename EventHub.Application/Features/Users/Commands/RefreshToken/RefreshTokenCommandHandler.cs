using EventHub.Application.Features.Users.Commands.RefreshToken;
using EventHub.Domain.Interfaces;
using MediatR;

namespace EventHub.Application.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByConditionAsync(u => u.RefreshToken == request.Token);

        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow || user.RefreshTokenRevoked != null)
        {
            throw new UnauthorizedAccessException("Nevažeći ili istekao refresh token.");
        }

        if (user.ReplacedByToken != null)
        {
            user.RefreshToken = null;
            await _unitOfWork.CompleteAsync();
            throw new UnauthorizedAccessException("Sigurnosno upozorenje: Token je kompromitovan.");
        }

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshTokenRevoked = DateTime.UtcNow; 
        user.ReplacedByToken = newRefreshToken;    

        user.RefreshToken = newRefreshToken;       
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.RefreshTokenCreated = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return new RefreshTokenResponse(newAccessToken, newRefreshToken);
    }
}