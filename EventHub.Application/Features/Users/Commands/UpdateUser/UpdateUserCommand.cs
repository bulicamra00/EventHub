using MediatR;

namespace EventHub.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    string FullName,
    string City,
    string Interests 
) : IRequest<Unit>;