
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Infrastructure.Data;
using SafeRouteAI.Infrastructure.Services;

namespace SafeRouteAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        // Routing, weather and risk services
        services.AddHttpClient<IWeatherService, WeatherService>(
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddHttpClient<IRouteService, RouteService>(
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
            });

        services.AddScoped<IRiskService, FakeRiskService>();
        services.AddScoped<RouteComparisonService>();

        // Database configuration
        // Preserve the existing connection-string behavior.
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = sp.GetService<IConfiguration>()
                ?.GetConnectionString("DefaultConnection")
                ?? AppDbContext.DefaultConnection;

            options.UseSqlServer(
                connectionString,
                sql => sql.EnableRetryOnFailure());
        });

        // Authentication dependencies
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<
            IPasswordHasher,
            Pbkdf2PasswordHasher>();

        services.AddSingleton<AuthMemoryState>();

        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<AuthService>();

        // Community reports
        services.AddScoped<
            ICommunityReportRepository,
            CommunityReportRepository>();

        services.AddScoped<CommunityReportService>();

        // Route request logging
        services.AddScoped<
            IRouteLogRepository,
            RouteLogRepository>();

        return services;
    }
}
