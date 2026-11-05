using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Tests;

public sealed class AppDbContextIntegrationTests
{
    [Fact]
    public void ModelContainsOnlyDomainEntitiesAndNoCheckConstraints()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=SafeRouteModelTest;User Id=test;Password=test;TrustServerCertificate=True")
            .Options;

        using var db = new AppDbContext(options);
        var expectedEntities = typeof(User).Assembly.GetTypes()
            .Where(type => type.IsClass &&
                           !type.IsAbstract &&
                           type.Namespace == typeof(User).Namespace)
            .ToHashSet();
        var modelEntities = db.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        Assert.Equal(expectedEntities, modelEntities.Select(entity => entity.ClrType).ToHashSet());
        Assert.All(modelEntities, entity => Assert.Empty(entity.GetCheckConstraints()));
        var humidity = modelEntities
            .Single(entity => entity.ClrType == typeof(WeatherLog))
            .FindProperty(nameof(WeatherLog.Humidity))!;
        Assert.True(humidity.IsNullable);
        Assert.Equal("humidity", humidity.GetColumnName());
        Assert.True(
            modelEntities.Single(entity => entity.ClrType == typeof(Route))
                .FindProperty(nameof(Route.UserID))!.IsNullable);
        AssertIndex<Route>(modelEntities, nameof(Route.UserID), nameof(Route.RequestDateTime));
        AssertIndex<RouteOption>(modelEntities, nameof(RouteOption.RouteID), nameof(RouteOption.Rank));
        AssertIndex<CommunityReport>(
            modelEntities,
            nameof(CommunityReport.ReportDateTime));
        AssertIndex<WeatherLog>(
            modelEntities,
            nameof(WeatherLog.Latitude),
            nameof(WeatherLog.Longitude),
            nameof(WeatherLog.DateTime));
        AssertIndex<AccidentRecord>(
            modelEntities,
            nameof(AccidentRecord.Latitude),
            nameof(AccidentRecord.Longitude),
            nameof(AccidentRecord.DateTime));
        AssertIndex<RouteRiskLog>(
            modelEntities,
            nameof(RouteRiskLog.RouteID),
            nameof(RouteRiskLog.LoggedAt));
        AssertIndex<RouteOptionRoadSegment>(
            modelEntities,
            nameof(RouteOptionRoadSegment.RouteOptionID),
            nameof(RouteOptionRoadSegment.SegmentOrder));
    }

    [SqlServerFact]
    [Trait("Category", "Integration")]
    public async Task DomainEntities_CanBeInsertedReadAndUpdated_AndRouteCanBeAnonymous()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SAFEROUTE_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "SAFEROUTE_TEST_CONNECTION_STRING was not set when the test was discovered.");

        var sqlConnectionString = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(sqlConnectionString.InitialCatalog) ||
            !sqlConnectionString.InitialCatalog.Contains(
                "test",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests require a dedicated database whose name contains 'Test'.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(sqlConnectionString.ConnectionString)
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();

        var accident = new AccidentRecord
        {
            Latitude = -26.2041m,
            Longitude = 28.0473m,
            DateTime = DateTime.UtcNow,
            AccidentType = "Integration test",
            Severity = "Low"
        };
        db.AccidentRecords.Add(accident);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var inserted = await db.AccidentRecords
            .AsNoTracking()
            .SingleAsync(record => record.AccidentID == accident.AccidentID);
        Assert.Equal("Low", inserted.Severity);

        var record = await db.AccidentRecords
            .SingleAsync(item => item.AccidentID == accident.AccidentID);
        record.Severity = "Moderate";
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var updated = await db.AccidentRecords
            .AsNoTracking()
            .SingleAsync(item => item.AccidentID == accident.AccidentID);
        Assert.Equal("Moderate", updated.Severity);

        var anonymousRoute = new Route
        {
            UserID = null,
            Origin = "Anonymous origin",
            Destination = "Anonymous destination",
            RequestDateTime = DateTime.UtcNow
        };
        db.Routes.Add(anonymousRoute);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var insertedAnonymousRoute = await db.Routes
            .AsNoTracking()
            .SingleAsync(route => route.RouteID == anonymousRoute.RouteID);
        Assert.Null(insertedAnonymousRoute.UserID);

        await transaction.RollbackAsync();
    }

    private static void AssertIndex<TEntity>(
        IEnumerable<IEntityType> entityTypes,
        params string[] propertyNames)
    {
        var entityType = Assert.Single(entityTypes, entity => entity.ClrType == typeof(TEntity));
        Assert.Contains(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(propertyNames));
    }
}

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("SAFEROUTE_TEST_CONNECTION_STRING")))
        {
            Skip = "Set SAFEROUTE_TEST_CONNECTION_STRING to run SQL Server integration tests.";
        }
    }
}
