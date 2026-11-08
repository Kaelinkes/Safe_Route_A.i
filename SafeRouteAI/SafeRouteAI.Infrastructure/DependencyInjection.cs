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
        services.AddHttpClient<IWeatherService, WeatherService>(
            client => client.Timeout = TimeSpan.FromSeconds(10));

        services.AddHttpClient<IRouteService, RouteService>(
            client => client.Timeout = TimeSpan.FromSeconds(30));

        services.AddScoped<IRiskService, FakeRiskService>();
        services.AddScoped<RouteComparisonService>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var configuration = sp.GetService<IConfiguration>();

            var connectionString =
                configuration?.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured.");

            options.UseSqlServer(
                connectionString,
                sql => sql.EnableRetryOnFailure());
        });

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<AuthMemoryState>();
        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<AuthService>();
        services.AddScoped<DevelopmentTestUserSeeder>();
        services.AddScoped<DevelopmentSampleDataSeeder>();

        services.AddScoped<ICommunityReportRepository, CommunityReportRepository>();
        services.AddScoped<CommunityReportService>();

        services.AddScoped<IRouteLogRepository, RouteLogRepository>();

        return services;
    }
}