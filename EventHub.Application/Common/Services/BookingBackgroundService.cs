using EventHub.Application.Features.Bookings.Commands.CancelExpiredBookings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Application.Common.Services;

public class BookingBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BookingBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task CancelExpiredBookings()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            await mediator.Send(new CancelExpiredBookingsCommand());
        }
    }
}