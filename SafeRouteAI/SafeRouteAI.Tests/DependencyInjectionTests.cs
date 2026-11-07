using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Infrastructure;
using SafeRouteAI.Infrastructure.Data;
using SafeRouteAI.Infrastructure.Services;

namespace SafeRouteAI.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureRegistersBackendServicesAndConfiguredSqlServerContext()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=localhost;Database=SafeRouteRegistrationTest;Trusted_Connection=True"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        Assert.IsAssignableFrom<IWeatherService>(scopedServices.GetRequiredService<IWeatherService>());
        Assert.IsAssignableFrom<IRouteService>(scopedServices.GetRequiredService<IRouteService>());
        Assert.IsType<FakeRiskService>(scopedServices.GetRequiredService<IRiskService>());
        Assert.IsType<RouteComparisonService>(scopedServices.GetRequiredService<RouteComparisonService>());
        Assert.IsType<CommunityReportService>(
            scopedServices.GetRequiredService<CommunityReportService>());
        Assert.IsType<CommunityReportRepository>(
            scopedServices.GetRequiredService<ICommunityReportRepository>());
        Assert.IsType<Pbkdf2PasswordHasher>(scopedServices.GetRequiredService<IPasswordHasher>());
        Assert.Same(TimeProvider.System, scopedServices.GetRequiredService<TimeProvider>());

        var db = scopedServices.GetRequiredService<AppDbContext>();
        Assert.Equal("SafeRouteRegistrationTest", db.Database.GetDbConnection().Database);
        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", db.Database.ProviderName);
    }

    [Fact]
    public void AddInfrastructureUsesDefaultSqlServerConnectionWithoutConfiguration()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal("SafeRouteAI", db.Database.GetDbConnection().Database);
    }
}
