using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Infrastructure.Persistence;
using SafeRouteAI.Infrastructure.Persistence.Entities;

namespace SafeRouteAI.Infrastructure.Tests;

public sealed class SafeRouteDbContextIntegrationTests
{
    [SqlServerFact]
    [Trait("Category", "Integration")]
    public async Task RiskCategory_CanBeInsertedReadAndUpdated()
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

        var categoryCode = $"t{Guid.NewGuid():N}"[..20];
        db.RiskCategories.Add(new RiskCategoryRecord
        {
            CategoryCode = categoryCode,
            SortOrder = 90
        });
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var inserted = await db.RiskCategories
            .AsNoTracking()
            .SingleAsync(category => category.CategoryCode == categoryCode);
        Assert.Equal((short)90, inserted.SortOrder);

        var category = await db.RiskCategories
            .SingleAsync(item => item.CategoryCode == categoryCode);
        category.SortOrder = 91;
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var updated = await db.RiskCategories
            .AsNoTracking()
            .SingleAsync(item => item.CategoryCode == categoryCode);
        Assert.Equal((short)91, updated.SortOrder);

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
