using MediatR;

namespace EventHub.Application.Features.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<RefreshTokenResponse>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);