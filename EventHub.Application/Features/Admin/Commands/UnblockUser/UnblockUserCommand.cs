using MediatR;

namespace EventHub.Application.Features.Admin.Commands.UnblockUser;

public record UnblockUserCommand(Guid UserId) : IRequest;