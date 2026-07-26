using EventHub.Application.Features.Users.Commands.LoginUser;
using EventHub.Domain.Interfaces;
using MediatR;
using BCrypt.Net;

namespace EventHub.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public LoginUserCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByConditionAsync(u => u.Email == request.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Neispravan email ili lozinka.");
        }

        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Email nije potvrđen. Molimo vas proverite vaš inbox.");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.RefreshTokenCreated = DateTime.UtcNow;
        
        user.RefreshTokenRevoked = null;
        user.ReplacedByToken = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return new LoginUserResponse(accessToken, refreshToken);
    }
}