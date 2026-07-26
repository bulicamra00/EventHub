using System.Reflection;
using FluentValidation;
using MediatR; 
using EventHub.Application.Behaviors; 
using Microsoft.Extensions.DependencyInjection;
using EventHub.Application.Mappings;
using EventHub.Application.Common.Services;

namespace EventHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient<TicketQrCodeResolver>();

        services.AddScoped<BookingBackgroundService>();

        return services;
    }
}