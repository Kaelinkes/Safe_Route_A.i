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
    private const string FallbackConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=SafeRouteDomainTest;Trusted_Connection=True;TrustServerCertificate=True";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Routing, weather, risk, comparison
        services.AddHttpClient<IWeatherService, WeatherService>(c => c.Timeout = TimeSpan.FromSeconds(10));
        services.AddHttpClient<IRouteService, RouteService>(c => c.Timeout = TimeSpan.FromSeconds(15));
        services.AddScoped<IRiskService, FakeRiskService>();
        services.AddScoped<RouteComparisonService>();

        // Database
        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
                ?? FallbackConnection));

        // Auth and reports
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<AuthMemoryState>();
        services.AddScoped<IUserStore, UserStore>();
        services.AddScoped<AuthService>();
        services.AddScoped<ICommunityReportRepository, CommunityReportRepository>();
        services.AddScoped<CommunityReportService>();
        services.AddScoped<IRouteLogRepository, RouteLogRepository>();

        return services;
    }
}