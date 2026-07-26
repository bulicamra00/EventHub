using EventHub.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EventHub.Infrastructure.Services;

public class AppConfig : IAppConfig
{
    private readonly IConfiguration _config;
    public AppConfig(IConfiguration config) => _config = config;

    public string FrontendUrl => _config["FrontendUrl"] ?? "http://localhost:5173";
}