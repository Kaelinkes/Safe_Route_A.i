# SQL Server persistence

`SafeRouteDbContext` maps the Domain entities directly to SQL Server tables.
Infrastructure references Domain, but Domain has no EF Core dependency. The
schema contains only Domain entities and their declared relationships; it does
not add lookup tables, seeded business data, check constraints, or database
defaults that enforce business rules. Primary keys and foreign keys reflect
entity identity and navigation properties.

The migration history was replaced with a fresh baseline to match the Domain
model. This baseline is intended for a new or disposable database. Do not apply
it to a database with data to preserve; recreate the disposable database before
running its integration test.

## Configure a local connection

From the solution directory, set a connection string with user-secrets. Substitute the host, user, and password for your SQL Server instance:

```sh
dotnet user-secrets set --project SafeRouteAI.API "ConnectionStrings:SafeRouteDatabase" "Server=localhost,1433;Database=SafeRouteAIDataBase;User Id=sa;Password=<Password123!Aa>;Encrypt=True;TrustServerCertificate=True"
```

Do not commit passwords or production connection strings. On macOS, SQL Server must be running in Docker or hosted remotely.

## Create and apply the database

From the solution directory, apply the checked-in Domain-aligned baseline to a
new or disposable database:

```sh
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update --project SafeRouteAI.Infrastructure --startup-project SafeRouteAI.API
```

For later Domain model changes, add a new migration with a distinct name and
review it before applying it:

```sh
dotnet ef migrations add <MigrationName> --project SafeRouteAI.Infrastructure --startup-project SafeRouteAI.API --output-dir Persistence/Migrations
```

At startup, the API validates that the connection string is configured and calls
EF Core `MigrateAsync`. If the configured database does not exist, the SQL Server
provider creates it, then applies pending migrations. The SQL login must be
allowed to connect to the SQL Server and create the database, and must have
permissions to apply schema changes. Startup fails if the server is unavailable
or migration application fails. You can also apply migrations explicitly with
`dotnet ef database update`.

The model uses SQL Server types inferred from the Domain property types,
identity columns for integer primary keys, and non-cascading foreign keys.
Review generated migrations before applying them to any shared database.

## Integration tests

See [the Infrastructure.Tests README](../SafeRouteAI.Infrastructure.Tests/README.md)
for configuring a disposable SQL Server test database and running the EF Core
insert/read/update integration test.

For the intended sample rows and relationships, see [the seed-data inventory](./SEED-DATA.md).