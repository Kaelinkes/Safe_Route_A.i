using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Infrastructure.Persistence;

namespace SafeRouteAI.Infrastructure.Tests;

public sealed class SafeRouteDbContextIntegrationTests
{
    [Fact]
    public void ModelContainsOnlyDomainEntitiesAndNoCheckConstraints()
    {
        var options = new DbContextOptionsBuilder<SafeRouteDbContext>()
            .UseSqlServer("Server=localhost;Database=SafeRouteModelTest;User Id=test;Password=test;TrustServerCertificate=True")
            .Options;

        using var db = new SafeRouteDbContext(options);
        var expectedEntities = typeof(User).Assembly.GetTypes()
            .Where(type => type.IsClass &&
                           !type.IsAbstract &&
                           type.Namespace == typeof(User).Namespace)
            .ToHashSet();
        var modelEntities = db.GetService<IDesignTimeModel>().Model.GetEntityTypes();

        Assert.Equal(expectedEntities, modelEntities.Select(entity => entity.ClrType).ToHashSet());
        Assert.All(modelEntities, entity => Assert.Empty(entity.GetCheckConstraints()));
    }

    [SqlServerFact]
    [Trait("Category", "Integration")]
    public async Task AccidentRecord_CanBeInsertedReadAndUpdated()
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

        var options = new DbContextOptionsBuilder<SafeRouteDbContext>()
            .UseSqlServer(sqlConnectionString.ConnectionString)
            .Options;

        await using var db = new SafeRouteDbContext(options);
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

        await transaction.RollbackAsync();
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
