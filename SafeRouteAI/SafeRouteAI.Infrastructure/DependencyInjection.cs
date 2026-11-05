using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.Infrastructure.Persistence;

namespace SafeRouteAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<SafeRouteDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("SafeRouteDatabase");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'SafeRouteDatabase' is missing. Configure it using user-secrets or the ConnectionStrings__SafeRouteDatabase environment variable.");
            }

            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
        });

        return services;
    }
}