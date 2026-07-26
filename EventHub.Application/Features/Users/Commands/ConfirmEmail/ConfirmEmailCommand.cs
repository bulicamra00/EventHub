using MediatR;

namespace EventHub.Application.Features.Users.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<bool>;