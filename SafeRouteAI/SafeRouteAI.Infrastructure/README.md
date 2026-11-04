# SQL Server persistence

`SafeRouteDbContext` maps the supplied route-risk schema to SQL Server. Persistence records live in Infrastructure so the Domain project stays independent of EF Core. Lookup rows, incident types, and risk factors are seeded by EF Core migrations.

## Configure a local connection

From the solution directory, set a connection string with user-secrets. Substitute the host, user, and password for your SQL Server instance:

```sh
dotnet user-secrets set --project SafeRouteAI.API "ConnectionStrings:SafeRouteDatabase" "Server=localhost,1433;Database=SafeRouteAIDataBase;User Id=sa;Password=<Password123!Aa>;Encrypt=True;TrustServerCertificate=True"
```

Do not commit passwords or production connection strings. On macOS, SQL Server must be running in Docker or hosted remotely.

## Create and apply the database

Install the EF command-line tool if needed, then run these commands from the solution directory:

```sh
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet tool install --global dotnet-ef --version 10.0.8
dotnet ef migrations add InitialCreate --project SafeRouteAI.Infrastructure --startup-project SafeRouteAI.API --output-dir Persistence/Migrations
dotnet ef database update --project SafeRouteAI.Infrastructure --startup-project SafeRouteAI.API
```

At startup, the API validates that the connection string is configured and calls
EF Core `MigrateAsync`. If the configured database does not exist, the SQL Server
provider creates it, then applies pending migrations. The SQL login must be
allowed to connect to the SQL Server and create the database, and must have
permissions to apply schema changes. Startup fails if the server is unavailable
or migration application fails. You can also apply migrations explicitly with
`dotnet ef database update`.

The model uses SQL Server `datetimeoffset`, `bit`, `nvarchar(max)`, identity columns, check constraints, filtered indexes, and explicit foreign-key delete behavior. Review each generated migration before applying it to a shared database.

## Integration tests

See [the Infrastructure.Tests README](../SafeRouteAI.Infrastructure.Tests/README.md)
for configuring a disposable SQL Server test database and running the EF Core
insert/read/update integration test.