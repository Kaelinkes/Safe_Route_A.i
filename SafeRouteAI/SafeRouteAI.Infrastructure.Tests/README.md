# SQL Server integration-test process

The infrastructure integration test exercises `AppDbContext` against a
real SQL Server database. It is different from an in-memory unit test: it
checks the SQL Server provider, migrations, and persisted data.

## What the current test verifies

`AppDbContextIntegrationTests.DomainEntities_CanBeInsertedReadAndUpdated_AndRouteCanBeAnonymous`
does the following:

1. Reads `SAFEROUTE_TEST_CONNECTION_STRING` from the test process environment.
2. Refuses to run against a database whose name does not contain `Test`
   (case-insensitive).
3. Applies pending EF Core migrations to that database.
4. Inserts a Domain `AccidentRecord` and saves it.
5. Reads it back without tracking and verifies its initial severity.
6. Updates the severity, saves, reads it back again, and verifies the change.
7. Inserts and reads a route with no user ID to confirm anonymous route requests
   are supported.
8. Rolls back the transaction so the test rows are not retained.

The companion model test verifies that EF includes exactly the Domain entity
types, defines no SQL check constraints, maps route ownership as nullable, and
contains the intended query indexes.

The migration step itself is not rolled back; schema changes remain in the
test database. The initial migration creates the Domain schema, and
`AllowAnonymousRoutes` makes `routes.user_id` nullable. Use a disposable test
database, never a production or important development database. If the
database already has the initial migration recorded, the pending nullable
column migration can be applied without recreating it. If tables exist without
matching EF migration history, inspect the schema and history before running
the test; do not reset a database with data to preserve.

## Prerequisites

- .NET 10 SDK.
- A running SQL Server instance reachable from the test machine.
- A dedicated database, for example `SafeRouteTestDatabase`.
- A SQL login allowed to connect to that database and apply migrations. If the
  database does not exist yet, the login also needs permission to create it.
- The database name must contain `Test`; the test checks this as a basic
  safeguard.

Prefer a dedicated test login with only the permissions needed for testing
instead of the SQL Server `sa` account. Do not commit credentials or paste
them into source files, command history, logs, or chat. Rotate any password
that has been exposed.

## Configure the test connection

From the repository root in zsh, enter the password without echoing it and
export a test-only connection string in the current terminal session:

```zsh
read -s "DB_PASSWORD?SQL Server test password: "
echo
export SAFEROUTE_TEST_CONNECTION_STRING="Server=localhost,1434;Database=SafeRouteTestDatabase;User Id=<test-login>;Password=${DB_PASSWORD};Encrypt=True;TrustServerCertificate=True"
unset DB_PASSWORD
```

Replace the host, port, database, and login with values for your test SQL
Server. Enter the password only at the prompt. The environment variable is
available to commands launched from that terminal; it is not saved by the
project, and it disappears when the terminal session ends.

## Run the integration test

Run the test project from the repository root:

```sh
dotnet test SafeRouteAI/SafeRouteAI.Infrastructure.Tests/SafeRouteAI.Infrastructure.Tests.csproj
```

The test uses the real connection string and applies any pending migrations
before performing its CRUD checks. A successful result should show one test
passed and no failures or skips.

If `SAFEROUTE_TEST_CONNECTION_STRING` is not set, xUnit reports the SQL Server
test as **skipped**. A skipped test is not evidence that the database works.

## Interpret common outcomes

| Result | Meaning / next check |
|---|---|
| Test passes | EF Core connected, migrations completed, and insert/read/update operations worked against the configured SQL Server database. The test row was rolled back. |
| `Login failed` | Confirm the instance/port and that the entered password is valid for the selected SQL login. Test that same instance independently with a SQL client. |
| Cannot open database | Verify the database name, that it exists or can be created, and that the login can access it. |
| Migration or permission error | The login may lack schema-change permission; migrations need DDL permissions on the test database. |
| Test skipped | The test process did not receive `SAFEROUTE_TEST_CONNECTION_STRING`. Export it in the same terminal that runs `dotnet test`. |
| Build succeeds but test fails | Build success only verifies compilation. Read the test failure; it may be a connection, schema, permission, constraint, or assertion problem. |

The repository may emit `NU1902` or `NU1903` package vulnerability warnings.
These warnings are separate from the test outcome and should be handled as
dependency maintenance work.

## Clean up the terminal environment

When finished, remove the connection string from the current shell:

```zsh
unset SAFEROUTE_TEST_CONNECTION_STRING
```

Keep the test database disposable. Drop or recreate it only when you are
certain it contains no data that needs to be retained.
