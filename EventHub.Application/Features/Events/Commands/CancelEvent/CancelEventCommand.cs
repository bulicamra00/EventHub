using MediatR; 

namespace EventHub.Application.Features.Events.Commands.CancelEvent;

public record CancelEventCommand(Guid EventId, string Reason) : IRequest<bool>;