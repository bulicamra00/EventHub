using EventHub.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using EventHub.Infrastructure.Services;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Persistence.Repositories;

namespace EventHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPaymentService, MockPaymentService>();
        services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ICsvService, CsvService>();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAppConfig, AppConfig>();

        return services;
    }
}