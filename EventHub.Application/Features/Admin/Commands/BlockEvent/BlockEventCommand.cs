using MediatR;

namespace EventHub.Application.Features.Admin.Commands.BlockEvent;

public record BlockEventCommand(Guid EventId, string Reason) : IRequest;