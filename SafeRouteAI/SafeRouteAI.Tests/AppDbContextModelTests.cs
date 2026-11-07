using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Tests;

public sealed class AppDbContextModelTests
{
    [Fact]
    public void ModelContainsDomainEntitiesKeysAndRelationships()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(AppDbContext.DefaultConnection)
            .Options;

        using var db = new AppDbContext(options);
        var entityTypes = db.Model.GetEntityTypes().ToDictionary(entity => entity.ClrType);
        var expectedTypes = typeof(Route).Assembly.GetTypes()
            .Where(type => type.IsClass &&
                           !type.IsAbstract &&
                           type.Namespace == typeof(Route).Namespace)
            .ToHashSet();

        Assert.Equal(expectedTypes.Count, entityTypes.Count);
        foreach (var type in expectedTypes)
        {
            Assert.Contains(type, entityTypes.Keys);
            Assert.NotNull(entityTypes[type].FindPrimaryKey());
        }

        AssertRelationship<RouteOption, Route>(entityTypes, nameof(RouteOption.RouteID));
        AssertRelationship<RouteOptionRoadSegment, RouteOption>(
            entityTypes,
            nameof(RouteOptionRoadSegment.RouteOptionID));
        AssertRelationship<RouteOptionRoadSegment, RoadSegment>(
            entityTypes,
            nameof(RouteOptionRoadSegment.RoadSegmentID));
        AssertRelationship<RouteRiskLog, Route>(entityTypes, nameof(RouteRiskLog.RouteID));
        AssertRelationship<Route, User>(entityTypes, nameof(Route.UserID));
        AssertRelationship<CommunityReport, User>(entityTypes, nameof(CommunityReport.UserID));
        AssertRelationship<CommunityReport, User>(
            entityTypes,
            nameof(CommunityReport.ReviewedByAdminID));
        AssertRelationship<RiskAssessment, RoadSegment>(
            entityTypes,
            nameof(RiskAssessment.RoadSegmentID));
        AssertRelationship<RiskAssessment, WeatherLog>(
            entityTypes,
            nameof(RiskAssessment.WeatherLogID));
        AssertRelationship<RiskExplanation, RiskAssessment>(
            entityTypes,
            nameof(RiskExplanation.RiskAssessmentID));

        Assert.Equal("weather_logs", entityTypes[typeof(WeatherLog)].GetTableName());
        Assert.Equal("humidity", entityTypes[typeof(WeatherLog)]
            .FindProperty(nameof(WeatherLog.Humidity))!
            .GetColumnName());
    }

    private static void AssertRelationship<TDependent, TPrincipal>(
        IReadOnlyDictionary<Type, Microsoft.EntityFrameworkCore.Metadata.IEntityType> entityTypes,
        string foreignKeyProperty)
    {
        Assert.Contains(
            entityTypes[typeof(TDependent)].GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(TPrincipal) &&
                          foreignKey.Properties.Select(property => property.Name)
                              .SequenceEqual(new[] { foreignKeyProperty }));
    }
}
