using MediatR;

namespace EventHub.Application.Features.Notifications.Commands.SendNotification;

public record SendNotificationCommand(Guid EventId, string Subject, string Message) : IRequest<bool>;