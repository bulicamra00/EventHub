using MediatR;

namespace EventHub.Application.Features.Admin.Commands.UnblockEvent;

public record UnblockEventCommand(Guid EventId) : IRequest;