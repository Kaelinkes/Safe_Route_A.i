using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Infrastructure.Services;

namespace SafeRouteAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<IWeatherService, WeatherService>(c => c.Timeout = TimeSpan.FromSeconds(10));
        return services;
    }
}