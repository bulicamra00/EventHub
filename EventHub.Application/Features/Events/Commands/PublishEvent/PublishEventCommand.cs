using MediatR;

namespace EventHub.Application.Features.Events.Commands.PublishEvent;

public record PublishEventCommand(Guid EventId) : IRequest<bool>;