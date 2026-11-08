# EF Core and SQL Server

For the full database table catalog, relationships, column conventions,
indexes, schema guarantees, migration history, and known gaps, see
[`DATABASE-DESIGN.md`](./DATABASE-DESIGN.md).

`AppDbContext` maps the Domain entities to SQL Server. Domain does not
depend on EF Core. The database stores entity data and relationships;
business calculations, validation, risk rules, and geometry encoding or
decoding belong in application code.

## Configure a development connection

The API reads `ConnectionStrings:DefaultConnection`. Keep credentials out of
source control and configure the connection with API user-secrets:

```sh
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=SafeRouteDatabase;User Id=sa;Password=<your-password>;Encrypt=True;TrustServerCertificate=True" \
  --project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj
```

Replace the server, database, login, and password with the intended
configuration. On macOS, SQL Server must be running in Docker or hosted
remotely. Do not commit or share passwords or full connection strings.

## Automatic migrations

Infrastructure registers `AppDbContext`; the API calls
`Database.MigrateAsync()` before accepting requests. EF Core creates a missing
database and applies unapplied migrations. A missing connection string,
unavailable SQL Server, or migration failure stops startup explicitly.

Run the API from the repository root:

```sh
dotnet run --project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj
```

Startup logs indicate when migrations are being applied and when they are up
to date. The SQL login must be permitted to create the database (if needed)
and apply schema changes.

For explicit migration application:

```sh
ConnectionStrings__DefaultConnection="<connection-string>" \
dotnet ef database update \
  --project SafeRouteAI/SafeRouteAI.Infrastructure/SafeRouteAI.Infrastructure.csproj \
  --startup-project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj
```

The design-time EF factory reads this environment variable; it does not read
API user-secrets.

Use startup migrations for local development and disposable test databases.
For production, review and deploy generated migration scripts as a controlled
deployment step instead of letting every API instance modify the schema.

## Development test accounts

When the API runs in the `Development` environment, startup inserts these
accounts if their email addresses do not already exist. Passwords are hashed
with the application password hasher. Existing accounts are not modified.

| Role | Email | Password |
|---|---|---|
| User | `safertest.user@example.test` | `TestUser123!` |
| Admin | `safertest.admin@example.test` | `TestAdmin123!` |

Use them with `POST /api/v1/auth/login`. These are public, predictable
development-only credentials; never enable this seeding behavior for a
production environment or reuse these passwords outside local testing.

Development startup also inserts the illustrative sample routes, road
segments, weather observations, risk assessments and explanations, route
options and risk logs, community reports, accident records, and model-version
metadata described in [`SEED-DATA.md`](./SEED-DATA.md). This seed is
idempotent and uses the test user/admin accounts above. It is not run in
production. The sample routes and risk data are not live routing or validated
safety information.

## Create later migrations

After an approved Domain or mapping change, scaffold and inspect a migration:

```sh
dotnet ef migrations add <MigrationName> \
  --project SafeRouteAI/SafeRouteAI.Infrastructure/SafeRouteAI.Infrastructure.csproj \
  --startup-project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj \
  --output-dir Data/Migrations
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
