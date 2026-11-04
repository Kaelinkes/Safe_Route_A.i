using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SafeRouteAI.Infrastructure.Data;
using SafeRouteAI.Application.Auth;

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
    options.UseSqlServer(
        sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
        ?? AppDbContext.DefaultConnection));

        services.AddScoped<ICommunityReportRepository, CommunityReportRepository>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        return services;
    }
}