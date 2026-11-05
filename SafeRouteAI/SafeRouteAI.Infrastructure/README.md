# EF Core and SQL Server

For the full database table catalog, relationships, column conventions,
indexes, schema guarantees, migration history, and known gaps, see
[`DATABASE-DESIGN.md`](./DATABASE-DESIGN.md).

`SafeRouteDbContext` maps the Domain entities to SQL Server. Domain does not
depend on EF Core. The database stores entity data and relationships;
business calculations, validation, risk rules, and geometry encoding or
decoding belong in application code.

## Configure a development connection

The API reads `ConnectionStrings:SafeRouteDatabase`. Keep credentials out of
source control. In zsh, enter the password without echoing it and export a
test-only connection string in the terminal used to start the API:

```zsh
read -s "DB_PASSWORD?SQL Server password: "
echo
export ConnectionStrings__SafeRouteDatabase="Server=localhost,1435;Database=SafeRouteDomainTest;User Id=sa;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True"
unset DB_PASSWORD
```

Replace the server, database, and login with the intended environment.
Alternatively, store the connection using API user-secrets. Do not commit or
share passwords or full connection strings. On macOS, SQL Server must be
running in Docker or hosted remotely.

## Automatic migrations

Infrastructure registers `SafeRouteDbContext`; the API calls
`Database.MigrateAsync()` before accepting requests. EF Core creates a missing
database and applies unapplied migrations. A missing connection string,
unavailable SQL Server, or migration failure stops startup explicitly.

Run the API from the repository root:

```sh
dotnet run --project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj
```

Startup logs should indicate it is applying migrations, followed by
`SafeRoute database is ready.` The SQL login must be permitted to create the
database (if needed) and apply schema changes.

For explicit migration application:

```sh
dotnet ef database update \
  --project SafeRouteAI/SafeRouteAI.Infrastructure/SafeRouteAI.Infrastructure.csproj \
  --startup-project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj
```

Use startup migrations for local development and disposable test databases.
For production, review and deploy generated migration scripts as a controlled
deployment step instead of letting every API instance modify the schema.

## Create later migrations

After an approved Domain or mapping change, scaffold and inspect a migration:

```sh
dotnet ef migrations add <MigrationName> \
  --project SafeRouteAI/SafeRouteAI.Infrastructure/SafeRouteAI.Infrastructure.csproj \
  --startup-project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj \
  --output-dir Persistence/Migrations
```

The current migration sequence is `DomainAlignedInitial`,
`AllowAnonymousRoutes`, `ContextQueryIndexes`, then
`AddHumidityToWeatherLogs`. If a database has tables but no matching EF
migration history, inspect its schema and history first: EF may otherwise try
to create tables that already exist. Never replace migration history or reset
a database containing data you need to keep. See the
[database design reference](./DATABASE-DESIGN.md#existing-databases-and-migration-history)
for safe recovery guidance.

## Related documentation

See the Infrastructure.Tests project documentation for SQL Server integration
testing. The intended sample seed rows are documented in
[`SEED-DATA.md`](./SEED-DATA.md).
