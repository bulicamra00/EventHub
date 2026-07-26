using MediatR;

namespace EventHub.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    string City
) : IRequest<Guid>; 