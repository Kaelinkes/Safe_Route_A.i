
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
        // Weather provider HTTP client.
        services.AddHttpClient<IWeatherService, WeatherService>(
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        // Real routing provider HTTP client.
        services.AddHttpClient<IRouteService, RouteService>(
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Backend risk service.
        // FakeRiskService remains active until real ML integration.
        services.AddScoped<IRiskService, FakeRiskService>();

        // Backend route comparison service.
        services.AddScoped<RouteComparisonService>();

        // SQL Server database context.
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = sp
                .GetService<IConfiguration>()?
                .GetConnectionString("DefaultConnection")
                ?? AppDbContext.DefaultConnection;

            options.UseSqlServer(
                connectionString,
                sql => sql.EnableRetryOnFailure());
        });

        // Authentication services.
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<AuthMemoryState>();
        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<AuthService>();

        // Community report services.
        services.AddScoped<
            ICommunityReportRepository,
            CommunityReportRepository>();

        services.AddScoped<CommunityReportService>();

        // Route logging repository.
        services.AddScoped<IRouteLogRepository, RouteLogRepository>();

        return services;
    }
}
