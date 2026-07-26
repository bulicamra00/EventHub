using MediatR;

namespace EventHub.Application.Features.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResponse>;

public record LoginUserResponse(string AccessToken, string RefreshToken);