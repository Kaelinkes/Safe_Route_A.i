# SafeRoute AI Database Documentation

## 1. Overview

The SafeRoute AI database is responsible for storing and managing the information required by the application. It provides the data storage foundation for user accounts, route requests, road networks, historical accidents, weather observations, community reports, and road risk assessments.

The database uses Microsoft SQL Server as its relational database management system and Entity Framework Core for object-relational mapping (ORM).

The database implementation is located in the `SafeRouteAI.Infrastructure` project. It follows a Code-First approach, meaning that database structures are defined and configured through C# entity classes and Entity Framework Core configurations. Migrations are then used to apply these changes to SQL Server.

The main objectives of the database are to:

- Store application data in a structured and organised format.
- Maintain relationships between related records.
- Protect data integrity through constraints and validation rules.
- Support route processing and road risk assessment.
- Maintain historical information for reporting and analysis.
- Provide a maintainable structure for future development.

## 2. Technologies Used

The database implementation uses the following technologies:

| Technology | Purpose |
|---|---|
| Microsoft SQL Server | Relational database management and data storage. |
| Entity Framework Core | Object-relational mapping and database operations. |
| C# | Definition of database entity classes and configurations. |
| EF Core Migrations | Tracking and applying database structure changes. |
| Visual Studio | Development and database integration. |
| SQL Server Management Studio (SSMS) | Inspecting and managing the SQL Server database. |
| .NET User Secrets | Managing local connection strings without committing credentials. |

## 3. Project Structure

The database implementation is located inside the `SafeRouteAI.Infrastructure` project.

```text
SafeRouteAI.Infrastructure
│
├── Persistence
│   ├── Entities
│   ├── Migrations
│   └── SafeRouteDbContext.cs
│
├── Class1.cs
├── DependencyInjection.cs
└── README.md
```

### Folder and File Responsibilities

**Persistence**

Contains the components responsible for storing and retrieving application data.

**Entities**

Contains the persistence record classes that represent the database tables. These classes define the properties and data structures used to store information.

**Migrations**

Contains Entity Framework Core migration files. These files record changes to the database structure and allow those changes to be applied to SQL Server.

**SafeRouteDbContext.cs**

The main database context. It inherits from Entity Framework Core's `DbContext` class and defines the database entities, table configurations, relationships, indexes, constraints, and initial reference data.

**DependencyInjection.cs**

Contains infrastructure service registration used by the application, including the registration of database-related services.

## 4. Database Design and Functional Areas

The database is organised into functional areas according to the responsibilities of the SafeRoute AI system.

### 4.1 User Accounts and Access Management

This area stores user account information and records related to account activity.

| Table | Description |
|---|---|
| `users` | Stores user details, email addresses, password hashes, roles, and account statuses. |
| `login_attempts` | Records login attempt information, including timestamps and failure details. |
| `user_sessions` | Stores session identifiers and hashed session tokens. |
| `ref_user_role` | Defines the available user roles. |
| `ref_account_status` | Defines account statuses such as active, suspended, and deleted. |

These tables provide the database structure needed to manage user information and account-related records.

### 4.2 Route Requests and Road Information

This area stores route requests and the road information used by the system.

| Table | Description |
|---|---|
| `route_requests` | Stores user route requests, including origin, destination, coordinates, and request status. |
| `candidate_routes` | Stores route options returned for a request, including distance, duration, ranking, and risk information. |
| `route_segments` | Connects candidate routes to the individual road segments they contain. |
| `road_segments` | Stores road information such as road names, road types, coordinates, speed limits, and road lengths. |
| `historical_accidents` | Stores historical accident information that can support road risk analysis. |
| `external_api_calls` | Records external service calls, including routing and weather requests. |
| `ref_request_status` | Defines the available route request statuses. |

These tables allow the application to maintain route information and associate route options with the relevant road network.

### 4.3 Weather and Community Reports

This area stores environmental information and incident reports submitted by community members.

| Table | Description |
|---|---|
| `weather_observations` | Stores weather measurements, coordinates, and observation validity periods. |
| `ref_weather_severity` | Defines weather severity levels. |
| `community_reports` | Stores incident reports submitted by users. |
| `incident_types` | Defines the different incident categories available for reporting. |
| `report_status_history` | Maintains a record of changes to community report statuses. |
| `ref_report_status` | Defines the available report statuses. |

Community reports can be associated with users and road segments. Their status information helps distinguish between reports that are pending, approved, rejected, expired, or resolved.

### 4.4 Risk Assessment and Prediction

This area stores the information required to maintain and explain road risk assessment results.

| Table | Description |
|---|---|
| `risk_model_versions` | Stores information about different versions of the risk prediction model. |
| `segment_risk_assessments` | Stores risk assessment results for individual road segments. |
| `assessment_community_reports` | Connects risk assessments to community reports considered during an assessment. |
| `risk_factors` | Stores the factors that can contribute to a risk assessment. |
| `assessment_factors` | Connects assessments to their contributing risk factors. |
| `ref_risk_category` | Defines the available risk categories. |

Risk factors can include weather conditions, historical accidents, community reports, road characteristics, time-related conditions, and data availability.

The database also supports insufficient-data results. This allows the system to distinguish between a calculated risk score and a situation where there is not enough information to produce a reliable assessment.

### 4.5 Warnings, Criteria, and Risk Logs

This area stores information used to identify high-risk conditions and maintain records of risk-related results.

| Table | Description |
|---|---|
| `high_risk_criteria` | Stores the criteria used to identify high-risk conditions. |
| `route_warnings` | Stores warnings associated with routes and road segments. |
| `route_risk_log` | Stores snapshots of route risk information for record-keeping and future reference. |
| `ref_external_service` | Defines the external services supported by the system. |

These records support the storage and retrieval of important risk information generated during route processing.

## 5. Database Context and Configuration

The database is configured through the `SafeRouteDbContext` class.

This class inherits from Entity Framework Core's `DbContext` class. It exposes database entities through `DbSet` properties, allowing Entity Framework Core to query and manage the corresponding database tables.

The `OnModelCreating` method defines how the database model should be created.

For easier maintenance, the configuration is divided into separate methods:

1. `ConfigureLookups()` configures reference tables and their initial data.
2. `ConfigureUsers()` configures user accounts, login attempts, and user sessions.
3. `ConfigureRoadAndRouteTables()` configures road information, route requests, candidate routes, and route segments.
4. `ConfigureSafetyTables()` configures weather observations, incident types, community reports, and report history.
5. `ConfigureRiskTables()` configures risk models, assessments, risk factors, warnings, and risk logs.

The context also converts C# property names from PascalCase to SQL snake_case.

For example:

- C# property: `UserId`
- SQL column: `user_id`

This provides a consistent naming convention between the C# persistence records and the SQL Server database.

## 6. Database Relationships and Entity Associations

The SafeRoute AI database uses primary keys and foreign keys to establish relationships between tables. These relationships ensure that related information is connected correctly and that records maintain their references to other records.

The relationships are configured in `SafeRouteDbContext.cs` using Entity Framework Core.

### 6.1 User and Account Relationships

| Parent table | Related table | Relationship | Description |
|---|---|---|---|
| `ref_user_role` | `users` | One-to-many | One role can be assigned to multiple users. |
| `ref_account_status` | `users` | One-to-many | One account status can be associated with multiple users. |
| `users` | `login_attempts` | One-to-many | A user can have multiple recorded login attempts. The user reference is optional. |
| `users` | `user_sessions` | One-to-many | One user can have multiple sessions. |

The role and account status tables act as reference tables. This helps prevent inconsistent role and status values from being stored in user records.

### 6.2 Route and Road Relationships

| Parent table | Related table | Relationship | Description |
|---|---|---|---|
| `users` | `route_requests` | One-to-many | One user can submit multiple route requests. |
| `ref_request_status` | `route_requests` | One-to-many | One request status can be used by multiple route requests. |
| `route_requests` | `external_api_calls` | One-to-many | One route request can have multiple external service calls. |
| `ref_external_service` | `external_api_calls` | One-to-many | One external service type can be referenced by multiple API call records. |
| `route_requests` | `candidate_routes` | One-to-many | One route request can produce multiple candidate routes. |
| `ref_risk_category` | `candidate_routes` | One-to-many | One risk category can be assigned to multiple candidate routes. |
| `candidate_routes` | `route_segments` | One-to-many | One candidate route can contain multiple road segments. |
| `road_segments` | `route_segments` | One-to-many | One road segment can be included in multiple candidate routes. |
| `road_segments` | `historical_accidents` | One-to-many | One road segment can be associated with multiple historical accidents. The road segment reference is optional. |

The `route_segments` table acts as a linking table between candidate routes and road segments. It also stores the sequence in which road segments appear within a route.

This means that candidate routes and road segments have a many-to-many relationship through `route_segments`.

### 6.3 Weather and Community Report Relationships

| Parent table | Related table | Relationship | Description |
|---|---|---|---|
| `ref_weather_severity` | `weather_observations` | One-to-many | One weather severity level can be assigned to multiple weather observations. |
| `users` | `community_reports` | One-to-many | One user can submit multiple community reports. |
| `incident_types` | `community_reports` | One-to-many | One incident type can be used by multiple community reports. |
| `road_segments` | `community_reports` | One-to-many | One road segment can be associated with multiple community reports. The road segment reference is optional. |
| `ref_report_status` | `community_reports` | One-to-many | One report status can be assigned to multiple community reports. |
| `users` | `community_reports` | One-to-many | A user can moderate multiple reports through the moderator reference. |
| `community_reports` | `report_status_history` | One-to-many | One community report can have multiple status history records. |
| `ref_report_status` | `report_status_history` | One-to-many | A report status can appear in multiple status history records. |
| `users` | `report_status_history` | One-to-many | One user can be associated with multiple report status changes. |

The `report_status_history` table maintains a record of changes made to community reports. It stores the previous status, the new status, the time of the change, and the user responsible for the change where provided.

### 6.4 Risk Assessment Relationships

| Parent table | Related table | Relationship | Description |
|---|---|---|---|
| `candidate_routes` | `segment_risk_assessments` | One-to-many | One candidate route can have multiple segment risk assessments. |
| `road_segments` | `segment_risk_assessments` | One-to-many | One road segment can be assessed multiple times. |
| `risk_model_versions` | `segment_risk_assessments` | One-to-many | One risk model version can be used for multiple assessments. |
| `weather_observations` | `segment_risk_assessments` | One-to-many | One weather observation can be referenced by multiple assessments. The weather reference is optional. |
| `ref_risk_category` | `segment_risk_assessments` | One-to-many | One risk category can be assigned to multiple assessments. |
| `segment_risk_assessments` | `assessment_community_reports` | One-to-many | One assessment can be linked to multiple community reports. |
| `community_reports` | `assessment_community_reports` | One-to-many | One community report can be considered in multiple assessments. |
| `segment_risk_assessments` | `assessment_factors` | One-to-many | One assessment can contain multiple contributing risk factors. |
| `risk_factors` | `assessment_factors` | One-to-many | One risk factor can contribute to multiple assessments. |

The `assessment_community_reports` table connects risk assessments with community reports. Similarly, `assessment_factors` connects assessments with the risk factors used to explain their results.

### 6.5 Warnings and Risk Management Relationships

| Parent table | Related table | Relationship | Description |
|---|---|---|---|
| `ref_weather_severity` | `high_risk_criteria` | One-to-many | One weather severity level can be referenced by multiple high-risk criteria records. |
| `users` | `high_risk_criteria` | One-to-many | One user can be recorded as responsible for updating multiple high-risk criteria records. |
| `candidate_routes` | `route_warnings` | One-to-many | One candidate route can have multiple warnings. |
| `road_segments` | `route_warnings` | One-to-many | One road segment can be associated with multiple warnings. |
| `high_risk_criteria` | `route_warnings` | One-to-many | One high-risk criterion can be referenced by multiple warnings. |
| `route_requests` | `route_risk_log` | One-to-many | One route request can have multiple risk log records. |
| `users` | `route_risk_log` | One-to-many | One user can be associated with multiple risk log records. The user reference is optional. |
| `risk_model_versions` | `route_risk_log` | One-to-many | One risk model version can be referenced by multiple risk log records. The model reference is optional. |

### 6.6 Many-to-Many Relationships

Some database relationships require linking tables because records from both sides can be associated with multiple records on the other side.

The database uses the following linking tables:

| Linking table | First table | Second table | Purpose |
|---|---|---|---|
| `route_segments` | `candidate_routes` | `road_segments` | Connects candidate routes to the road segments they contain. |
| `assessment_community_reports` | `segment_risk_assessments` | `community_reports` | Connects risk assessments to the community reports considered during assessment. |
| `assessment_factors` | `segment_risk_assessments` | `risk_factors` | Connects risk assessments to their contributing risk factors. |

These linking tables use composite primary keys to prevent duplicate associations between the same records.

### 6.7 Understanding the Relationship Structure

The relationships support the general flow of information through the SafeRoute AI system:

1. A registered user submits a route request.
2. The route request can produce multiple candidate routes.
3. Each candidate route is divided into road segments.
4. Road segments can be associated with historical accidents and community reports.
5. Weather observations provide additional environmental information.
6. Risk assessments evaluate individual road segments within candidate routes.
7. Assessments can be connected to relevant community reports and risk factors.
8. Route warnings and risk logs maintain additional information about risk results.

This structure allows information from different parts of the application to be connected while maintaining clear relationships between database records.

## 7. Data Integrity and Validation

The database uses primary keys, foreign keys, check constraints, unique indexes, and delete behaviours to maintain data integrity.

### 7.1 Primary Keys and Foreign Keys

Primary keys uniquely identify records within their respective tables.

Foreign keys establish relationships between tables and help prevent records from referencing non-existent related records.

For example, a candidate route must reference an existing route request.

### 7.2 Check Constraints

Check constraints are used to prevent invalid values from being stored.

Examples include:

- Latitude values must fall between -90 and 90.
- Longitude values must fall between -180 and 180.
- Road lengths and route distances must be greater than zero.
- Risk scores must fall between 0 and 100 when provided.
- Humidity values must remain within the valid percentage range.
- Route origin and destination coordinates cannot be identical.
- Processing times and other non-negative measurements cannot contain negative values.
- Community reports must satisfy the configured description and expiry requirements.

### 7.3 Unique Indexes

Unique indexes help prevent duplicate records where uniqueness is required.

Examples include:

- Normalised user email addresses.
- External road segment references.
- Risk model name and version combinations.
- Risk factor codes.
- Active risk model records.
- Route rankings within the same request where a ranking is provided.

### 7.4 Delete Behaviour

Entity Framework Core delete behaviours control what happens when a related record is deleted.

The database uses behaviours such as:

- **Cascade:** Deletes dependent records when their parent record is deleted.
- **Restrict:** Prevents deletion when dependent records still reference the parent.
- **SetNull:** Sets an optional foreign key to null when the referenced record is deleted.
- **NoAction:** Prevents automatic cascading behaviour for selected relationships.

These settings help maintain database integrity and reduce the possibility of accidentally deleting important information.

## 8. Reference Data and Seeding

Reference tables store standard values used throughout the application.

Examples include user roles, account statuses, route request statuses, report statuses, weather severity levels, risk categories, and external service types.

Entity Framework Core uses the `HasData()` method to define initial records for supported lookup entities.

Initial data is also configured for incident types and risk factors.

This approach helps maintain consistency by allowing different parts of the application to use predefined values rather than introducing inconsistent values manually.

Reference data changes are tracked through EF Core migrations.

## 9. Configure a Local Connection

The application uses a SQL Server connection string to establish a connection to the database.

For local development, connection strings should be configured through .NET User Secrets rather than being stored directly in source code.

From the solution directory, run:

```sh
dotnet user-secrets set --project SafeRouteAI.API "ConnectionStrings:SafeRouteDatabase" "Server=localhost,1433;Database=SafeRouteAIDataBase;User Id=sa;Password=YOUR_STRONG_PASSWORD;Encrypt=True;TrustServerCertificate=True"
```

Replace the server address and credentials with the details of your local SQL Server instance.

The example uses SQL Server authentication. Developers using Windows authentication or a different SQL Server configuration should adjust the connection string accordingly.

Do not commit passwords, private connection strings, or production database credentials to GitHub.

On macOS, SQL Server must be running in Docker or hosted remotely.

## 10. Create and Apply the Database

Entity Framework Core migrations are used to create and update the database structure.

If the Entity Framework Core command-line tool is not installed, it can be installed using:

```sh
dotnet tool install --global dotnet-ef --version 10.0.8
```

If the tool is already installed, make sure its version is compatible with the project's Entity Framework Core packages.

From the solution directory, the following commands can be used.

### 10.1 Add a Migration

```sh
dotnet ef migrations add InitialCreate --project SafeRouteAI.Infrastructure --startup-project SafeRouteAI.API --output-dir Persistence/Migrations
```

This creates a migration based on the current entity classes and database configuration.

Migration names should describe the database change being introduced.

### 10.2 Apply Migrations

```sh
dotnet ef database update --project SafeRouteAI.Infrastructure --startup-project SafeRouteAI.API
```

This applies pending migrations to the configured SQL Server database.

### 10.3 Review Migration Changes

Before applying a migration to a shared database, developers should review the generated migration files.

This helps identify changes such as:

- New tables or columns.
- Removed or modified columns.
- New indexes and constraints.
- Changes to foreign keys.
- Changes to default values and data types.

Existing migrations should not be deleted or rewritten simply to introduce a new change. New database changes should normally be introduced through additional migrations.

## 11. Application Startup and Automatic Migrations

At startup, the API validates that a connection string has been configured and calls Entity Framework Core's `MigrateAsync` method.

This allows pending migrations to be applied automatically when the application starts.

If the configured database does not exist, the SQL Server provider can create it when the connecting SQL login has the required permissions.

The database login must have permission to connect to SQL Server, create the database when necessary, and apply schema changes.

If the SQL Server instance is unavailable or migration application fails, application startup fails.

Developers can also apply migrations manually using the `dotnet ef database update` command.

Automatic migrations are convenient during development. For shared or production environments, database changes should be reviewed and deployed through an agreed migration process.

## 12. SQL Server Data Types and Features

The database uses SQL Server data types and features to represent information correctly.

The configured model uses features such as:

- `datetimeoffset` for date and time information that includes an offset.
- `bit` for Boolean values.
- `nvarchar(max)` for variable-length text that may require additional storage.
- Identity columns for automatically generated numeric identifiers.
- Check constraints for validating stored values.
- Filtered indexes for applying uniqueness rules to selected records.
- Explicit foreign-key delete behaviours for managing relationships.

These features help support the database requirements while making use of SQL Server functionality.

## 13. Database Security Considerations

Database security is important because SafeRoute AI stores user information, session information, route history, and community reports.

The database configuration includes measures that support data protection and integrity:

- Password information is represented by a password hash field rather than a plain-text password field.
- Session tokens are represented by a token hash field.
- Foreign keys help prevent invalid relationships.
- Check constraints restrict invalid values.
- Unique indexes prevent duplicate records where required.
- Delete behaviours help control the removal of related records.
- UTC-based database defaults are used for several automatically generated timestamps.
- Local connection credentials are managed using User Secrets.

These measures form part of the database security approach. Password hashing, authentication, session validation, and access permissions also require appropriate implementation in the application and deployment environment.

The database login should only receive the permissions required for its intended purpose. Production credentials should not be shared or committed to the repository.

## 14. Integration Tests

Database integration tests are used to check whether the persistence implementation can interact with SQL Server correctly.

These tests help verify that database operations work as expected, including inserting, reading, and updating records.

The Infrastructure.Tests project contains the database integration test implementation.

For instructions on configuring a disposable SQL Server test database and running the EF Core insert, read, and update integration test, refer to:

[Infrastructure.Tests README](../SafeRouteAI.Infrastructure.Tests/README.md)

A disposable test database should be used where possible to prevent test operations from affecting shared development or production data.

## 15. Development Team Guidelines

Because SafeRoute AI is developed collaboratively, database changes should be handled carefully to maintain consistency across the project.

Before introducing a database change, developers should:

1. Pull the latest approved changes from the shared GitHub repository.
2. Review the existing entity classes and database configuration.
3. Check whether another developer has already introduced a similar change.
4. Discuss changes that may affect other project components.
5. Make the required database modification.
6. Generate a new migration when necessary.
7. Review the generated migration before applying it.
8. Build and test the project.
9. Verify that the database structure and relationships still work correctly.
10. Commit and push the changes to the appropriate Git branch.

This process helps reduce merge conflicts and prevents unnecessary changes to database structures already being used by other parts of the application.

## 16. Conclusion

The SafeRoute AI database provides the data storage foundation for the application. It manages information relating to users, routes, road segments, historical accidents, weather observations, community reports, and risk assessments.

By using Microsoft SQL Server and Entity Framework Core, the project follows a structured approach to database management.

The use of entity configurations, relationships, indexes, constraints, reference data, and migrations helps maintain data consistency and supports future development.

As the project continues to grow, the database should be reviewed and maintained alongside the other system components to ensure that it remains reliable, secure, and aligned with the requirements of SafeRoute AI.
