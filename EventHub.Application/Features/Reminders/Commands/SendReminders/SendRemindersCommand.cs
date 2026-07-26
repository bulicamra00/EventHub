using MediatR;

namespace EventHub.Application.Features.Reminders.Commands.SendReminders;

public record SendRemindersCommand : IRequest;