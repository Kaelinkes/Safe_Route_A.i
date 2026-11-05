# SafeRouteAI database design

This document records the SQL Server schema and persistence behavior currently
implemented by `SafeRouteDbContext`. It describes the code and migrations in
this checkout, not every feature proposed in project context documents.

## Architecture and ownership

- `SafeRouteAI.Domain` defines the persisted entity types and their properties.
- `SafeRouteAI.Infrastructure` maps those types to SQL Server with EF Core.
- `SafeRouteAI.API` configures the connection and applies pending migrations
  before starting to serve requests.
- The database enforces relational structure (primary keys, foreign keys,
  nullability, and indexes). Business validation, risk calculations, status
  transitions, and geometry encoding/decoding belong in application code.
- EF Core SQL Server and tooling packages currently target .NET 10 / EF Core
  10.0.8.

The context maps each entity to a plural, snake_case table name and each
property to a snake_case column name. Primary keys are integer identity
columns, except for the route-option/road-segment join table, whose primary
key is composite.

## Entity and table catalog

All C# `string` properties currently map to SQL Server `nvarchar(max)` unless
otherwise noted. Required/optional here describes the current CLR/EF mapping;
the database does not impose additional allowed-value lists.

| Table | Purpose and persisted data |
|---|---|
| `users` | `user_id` identity PK; required `name`, `email`, `password_hash`, `role`, and `date_registered`. No unique email index is configured. Password hashes must be produced and verified by application authentication code. |
| `routes` | `route_id` identity PK; nullable `user_id` FK for optional/anonymous ownership; required `origin`, `destination`, and `request_date_time`. |
| `route_options` | `route_option_id` identity PK; required `route_id` FK, `route_info`, `geometry`, `distance`, `est_time_min`, and `rank`; optional `risk_score`. `geometry` is an unvalidated string. |
| `road_segments` | `road_segment_id` identity PK; required start/end latitude and longitude (`decimal(9,6)`); optional `road_type` and `road_name`. The four coordinate fields represent endpoints. |
| `route_option_road_segments` | Join table for route options and road segments; composite PK (`route_option_id`, `road_segment_id`), both required FKs, and required `segment_order`. |
| `weather_logs` | `weather_log_id` identity PK; required latitude/longitude (`decimal(9,6)`) and `date_time`; optional `precipitation`, `humidity`, `visibility`, and `wind_speed` (`float`). Units and provider are not stored. |
| `risk_assessments` | `risk_assessment_id` identity PK; required `road_segment_id` FK; optional `weather_log_id` FK, `risk_score` (`float`), and `risk_level`; required `assessment_status`. This is a road-segment-level assessment. |
| `risk_explanations` | `risk_explanation_id` identity PK; required `risk_assessment_id` FK and `explanation_text`. A unique index on the FK enforces at most one explanation per assessment. |
| `community_reports` | `report_id` identity PK; required `user_id` FK, location (`decimal(9,6)`), `incident_type`, `report_date_time`, and `status`; optional `description`, `reviewed_by_admin_id` FK to `users`, and `reviewed_at`. |
| `accident_records` | `accident_id` identity PK; required location (`decimal(9,6)`) and `date_time`; optional `accident_type` and `severity`. |
| `model_versions` | `model_version_id` identity PK; required `version_label`, `training_date`, `dataset_version`, `model_path`, and `validation_summary`. Stores metadata, not model files. |
| `route_risk_logs` | `route_risk_log_id` identity PK; required `route_id` FK, `logged_at`, and `result_snapshot` string. The snapshot format is not validated by SQL Server. |

EF Core uses SQL Server's `datetime2` for `DateTime`, `float` for C# `double`,
`int` for identifiers/order fields, and `decimal(9,6)` for the configured
coordinates. Date/time values have no database-enforced timezone convention;
the application should consistently use UTC where intended.

## Relationships

```text
users 1 ─── * routes                         (route user is optional)
routes 1 ─── * route_options
route_options * ─── * road_segments          (through route_option_road_segments)
road_segments 1 ─── * risk_assessments
weather_logs 1 ─── * risk_assessments        (assessment weather is optional)
risk_assessments 1 ─── 0..1 risk_explanations
users 1 ─── * community_reports              (reporter)
users 1 ─── * community_reports              (optional reviewer)
routes 1 ─── * route_risk_logs
```

Foreign-key delete behavior is `NoAction`; deletes are not configured to
cascade through the relationships. The route-option/segment many-to-many
relationship is explicitly represented by its join entity so segment order
can be stored.

## Indexes and schema guarantees

Non-unique indexes support the modeled access paths:

| Index columns | Table | Intended access path |
|---|---|---|
| `user_id, request_date_time` | `routes` | A user's route history ordered/filtered by request time; the leading column also supports user-FK lookup. |
| `route_id, rank` | `route_options` | Options for a route in ranking order. |
| `route_option_id, segment_order` | `route_option_road_segments` | Segments of an option in route order. |
| `road_segment_id` | `route_option_road_segments` | Reverse lookup by road segment. |
| `latitude, longitude, date_time` | `weather_logs` | Weather observations by coordinate and time. |
| `latitude, longitude, date_time` | `accident_records` | Accident records by coordinate and time. |
| `report_date_time` | `community_reports` | Report chronology. |
| `user_id` | `community_reports` | Reports by submitting user. |
| `reviewed_by_admin_id` | `community_reports` | Reports by reviewer. |
| `road_segment_id` | `risk_assessments` | Assessments by road segment. |
| `weather_log_id` | `risk_assessments` | Assessments by weather observation. |
| `risk_assessment_id` (unique) | `risk_explanations` | One-to-one assessment explanation relationship. |
| `route_id, logged_at` | `route_risk_logs` | Route risk history by time. |

The indexes do not represent status or score validation. The schema currently
does not define business check constraints, unique email enforcement, status
lookup tables, or EF seed data. Defaults on entity properties (such as
`CommunityReport.Status`) are application-side initial values, not SQL
defaults. Community report status strings, roles, assessment statuses, score
ranges, coordinate ranges, and unit conventions are not restricted by the
database.

## Migration history

Migrations live in `Persistence/Migrations`:

1. `20261105101358_DomainAlignedInitial` creates the tables and relational
   constraints for the Domain model.
2. `20261105121313_AllowAnonymousRoutes` makes `routes.user_id` nullable. Its
   `Down` migration refuses to reverse this change while anonymous route rows
   exist.
3. `20261105122832_ContextQueryIndexes` replaces selected FK-only indexes
   with composite indexes and adds the query indexes listed above. It does not
   alter column types or data.
4. `20261105130738_AddHumidityToWeatherLogs` adds the optional `humidity`
   (`float`) column to `weather_logs`.

The model snapshot represents the current model. When changing a Domain entity
or EF mapping, scaffold a migration, inspect it for unintended data/type
changes, and check that the model snapshot is current before applying it.

## Startup and connection configuration

`AddInfrastructure()` registers `SafeRouteDbContext` with SQL Server and
transient retry support. The context resolves
`ConnectionStrings:SafeRouteDatabase` from the host's `IConfiguration` when it
is first created; a missing connection string throws an explicit
configuration exception. In API `Program.cs`, `Database.MigrateAsync()` runs
before the app starts listening; a database connection or migration failure
therefore prevents startup.

Use API user-secrets or the `ConnectionStrings__SafeRouteDatabase`
environment variable. For example, from zsh, prompt for a development
password without echoing or embedding it in the command:

```zsh
read -s "DB_PASSWORD?SQL Server password: "
echo
export ConnectionStrings__SafeRouteDatabase="Server=localhost,1433;Database=SafeRouteDatabase;User Id=sa;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True"
unset DB_PASSWORD
```

Replace the server, database, and login with the intended environment. Never
commit or share credentials; rotate any credential that has been exposed.
For production, deploy reviewed migration scripts through a controlled
deployment process rather than allowing every API instance to mutate schema.

## Existing databases and migration history

EF Core tracks applied migrations in `__EFMigrationsHistory`. `MigrateAsync`
applies migrations missing from that history; it does not inspect arbitrary
existing tables and infer that they match a migration. If a database already
contains tables such as `road_segments` but has no matching migration history,
startup may fail with “There is already an object named ...”. Do not work
around this by blindly dropping tables or inserting migration-history rows.

For a disposable database, confirm the exact connection target and that its
contents may be erased before recreating it, then apply migrations. For a
database with data to preserve, back it up and compare its schema and data
with the migration before planning a reviewed baseline/adoption or
data-preserving migration.

## Schema scope and known gaps

- `WeatherLog` does not persist temperature, weather condition/code, units, or
  provider provenance.
- Risk assessments are related to road segments, not route options. The
  correct assessment grain should be agreed before changing this relationship.
- `RouteOption.Geometry` is just `nvarchar(max)`; the database does not
  distinguish GeoJSON from encoded polyline. Encode/decode and format
  validation belong in application code.
- There are no authentication-session, login-attempt, token-hash persistence,
  or concrete user-store tables in this Domain model.
- The current API host has a sample weather forecast endpoint; route
  comparison, authentication, and report workflows described in broader
  project context are not necessarily implemented by this branch.
- The sample rows in `SEED-DATA.md` describe a conversationally supplied seed
  script; they are not automatically inserted by EF migrations and do not
  prove that a particular database contains those rows.

## Tests and related references

The Infrastructure.Tests project has a SQL Server integration test that
applies migrations, checks insert/read/update behavior, and verifies anonymous
route persistence. It requires a dedicated disposable database and
`SAFEROUTE_TEST_CONNECTION_STRING`; see
[`Infrastructure.Tests/README.md`](../SafeRouteAI.Infrastructure.Tests/README.md)
for setup and safeguards. See [`SEED-DATA.md`](./SEED-DATA.md) for the
illustrative sample data.
