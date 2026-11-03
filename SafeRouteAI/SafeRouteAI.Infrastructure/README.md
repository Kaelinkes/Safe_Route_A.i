# SQL Server persistence

`SafeRouteDbContext` maps the supplied route-risk schema to SQL Server. Persistence records live in Infrastructure so the Domain project stays independent of EF Core. Lookup rows, incident types, and risk factors are seeded by EF Core migrations.

## Configure a local connection

From the solution directory, set a connection string with user-secrets. Substitute the host, user, and password for your SQL Server instance:

```sh
dotnet user-secrets set --project SafeRouteAI.API "ConnectionStrings:SafeRouteDatabase" "Server=localhost,1433;Database=SafeRouteAI;User Id=sa;Password=<your-password>;Encrypt=True;TrustServerCertificate=True"
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

The API validates that the connection string is configured at startup. Database updates are applied explicitly with `dotnet ef database update`; the application does not mutate the database schema automatically at startup.

The model uses SQL Server `datetimeoffset`, `bit`, `nvarchar(max)`, identity columns, check constraints, filtered indexes, and explicit foreign-key delete behavior. Review each generated migration before applying it to a shared database.