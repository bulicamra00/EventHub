using EventHub.Application.Features.Reminders.Commands.SendReminders;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Application.Common.Services;

public class ReminderBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReminderBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SendReminders()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new SendRemindersCommand());
        }
    }
}