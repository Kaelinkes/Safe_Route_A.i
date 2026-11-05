using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Infrastructure.Data;
using SafeRouteAI.Infrastructure.Services;

namespace SafeRouteAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<IWeatherService, WeatherService>(c => c.Timeout = TimeSpan.FromSeconds(10));
        services.AddHttpClient<IRouteService, RouteService>(c => c.Timeout = TimeSpan.FromSeconds(15));
        services.AddScoped<IRiskService, FakeRiskService>();
        services.AddScoped<RouteComparisonService>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = sp.GetService<IConfiguration>()
                ?.GetConnectionString("DefaultConnection")
                ?? AppDbContext.DefaultConnection;

            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
        });

        services.AddScoped<CommunityReportService>();
        services.AddScoped<ICommunityReportRepository, CommunityReportRepository>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}