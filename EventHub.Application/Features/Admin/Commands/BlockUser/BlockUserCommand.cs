using MediatR;

namespace EventHub.Application.Features.Admin.Commands.BlockUser;

public record BlockUserCommand(Guid UserId, string Reason) : IRequest;