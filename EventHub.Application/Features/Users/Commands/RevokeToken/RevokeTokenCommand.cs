using MediatR;

namespace EventHub.Application.Features.Users.Commands.RevokeToken;

public record RevokeTokenCommand(string Token) : IRequest;