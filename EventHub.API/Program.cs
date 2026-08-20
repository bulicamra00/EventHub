using EventHub.Application;
using EventHub.Infrastructure;
using EventHub.Infrastructure.Persistence;
using EventHub.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Services;
using Hangfire;
using EventHub.Application.Common.Services; 

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://seq:5341") 
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://63.187.61.129:5173") 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventHub API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Unesite JWT token: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

app.UseCors("AllowReactApp");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireCustomAuthFilter() } 
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    
    int maxRetries = 10;
    int delaySeconds = 3;
    bool connected = false;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            dbContext.Database.CanConnect();
            connected = true;
            Log.Information("Uspešno uspostavljena veza sa bazom podataka.");
            
            // Automatski primeni migracije i kreiraj bazu/tabele
            dbContext.Database.Migrate();
            Log.Information("Migracije baze su uspešno primenjene.");
            
            break;
        }
        catch (Exception)
        {
            Log.Warning($"Baza još nije spremna. Pokušaj {i + 1}/{maxRetries}. Čekam {delaySeconds} sekundi...");
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }

    if (connected)
    {
        recurringJobManager.AddOrUpdate<BookingBackgroundService>(
            "cancel-expired-bookings", 
            x => x.CancelExpiredBookings(), 
            "*/10 * * * *");

        recurringJobManager.AddOrUpdate<ReminderBackgroundService>(
            "send-event-reminders", 
            x => x.SendReminders(), 
            "*/30 * * * *"); 
    }
    else
    {
        Log.Error("Nije moguće uspostaviti vezu sa bazom podataka. Hangfire poslovi nisu zakazani.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventHub API V1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();  
app.MapControllers();

app.Run();

public class HangfireCustomAuthFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context) => true;
}