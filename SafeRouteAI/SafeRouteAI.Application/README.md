# SafeRouteAI Backend

## Overview

The **SafeRouteAI Backend** is the server-side component of the SafeRouteAI system. It provides RESTful API endpoints, manages application data, handles authentication, processes route information, and supports route risk assessment.

The backend serves as the communication layer between the frontend, database, and external services, ensuring reliable data processing and application functionality.

## Technologies Used

| Technology | Purpose |
|---|---|
| **C#** | Main programming language. |
| **ASP.NET Core Web API** | RESTful API development and HTTP request handling. |
| **Entity Framework Core** | Object-relational mapping (ORM) and database operations. |
| **Microsoft SQL Server** | Relational database management. |
| **HttpClient** | Communication with external APIs and services. |
| **.NET Dependency Injection** | Service registration and dependency management. |
| **xUnit** | Automated unit and integration testing. |

## Architecture

The backend follows a **layered architecture** to separate responsibilities, improve maintainability, and support scalability.

### 1. SafeRouteAI.API

Responsible for handling HTTP requests and exposing API endpoints.

- Controllers
- Request and response DTOs
- Authentication handling
- API service integrations
- Error handling

### 2. SafeRouteAI.Application

Contains the application's business logic and service contracts.

- Application services
- Data Transfer Objects (DTOs)
- Interfaces
- Authentication services
- Route comparison and risk assessment logic

### 3. SafeRouteAI.Domain

Contains the core business entities and enumerations.

- Database entities
- Risk levels
- Assessment statuses
- Community report statuses

### 4. SafeRouteAI.Infrastructure

Responsible for database access and external service implementations.

- Entity Framework Core DbContext
- Database repositories
- Route services
- Weather services
- Password hashing
- Dependency injection configuration

## Main Backend Features

### Authentication

- User authentication.
- Secure password hashing.
- Session-based authentication.

### Route Management

- Retrieves available routes between two coordinates.
- Processes route distances and estimated travel durations.
- Compares alternative routes.
- Maps and transforms backend route responses.

### Risk Assessment

- Evaluates route risk levels.
- Processes risk assessment results.
- Supports integration with machine learning services.

### Weather Integration

- Retrieves weather information.
- Processes weather conditions for route risk evaluation.

### Community Reports

- Allows community members to submit incident reports.
- Retrieves pending and approved reports.
- Supports administrator moderation.
- Enables report status updates.

### Route Logging

- Saves route requests.
- Stores origin and destination coordinates.
- Records route information and risk assessment results.

## Project Structure

```text
SafeRouteAI/
│
├── SafeRouteAI.API/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Security/
│   └── Services/
│
├── SafeRouteAI.Application/
│   ├── Auth/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Interfaces/
│   └── Services/
│
├── SafeRouteAI.Domain/
│   ├── Entities/
│   └── Enums/
│
├── SafeRouteAI.Infrastructure/
│   ├── Data/
│   └── Services/
│
├── SafeRouteAI.API.Tests/
├── SafeRouteAI.Infrastructure.Tests/
├── SafeRouteAI.Tests/
│
└── SafeRouteAI.slnx
```

## Requirements

Before running the backend, ensure the following are installed:

- [.NET SDK](https://dotnet.microsoft.com/download) compatible with the solution.
- Microsoft SQL Server or SQL Server LocalDB.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/).
- [Git](https://git-scm.com/).

## Setup and Installation

### 1. Clone the Repository

```bash
git clone YOUR_REPOSITORY_URL
```

### 2. Navigate to the Backend Project

```bash
cd SafeRouteAI
```

### 3. Restore Dependencies

```bash
dotnet restore SafeRouteAI.slnx
```

### 4. Configure the Database

Configure `ConnectionStrings:DefaultConnection` with API user-secrets for your
local SQL Server instance. Do not put credentials in `appsettings.json`.

```sh
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=YOUR_SERVER;Database=SafeRouteAI;User Id=YOUR_LOGIN;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True" \
  --project SafeRouteAI/SafeRouteAI.API/SafeRouteAI.API.csproj
```

Replace the server, database, login, and password with your SQL Server
configuration. Ensure that SQL Server is running and accessible before
starting the API.

### 5. Build the Solution

```bash
dotnet build SafeRouteAI.slnx
```

### 6. Run the Backend API

```bash
dotnet run --project SafeRouteAI.API
```

Once successfully started, the API will display its local address in the terminal.

## Running Tests

To execute the complete automated test suite, run:

```bash
dotnet test SafeRouteAI.slnx
```

The automated tests cover:

- API functionality.
- Authentication.
- Route comparison.
- Risk service integration.
- Error handling.
- Weather services.
- Database and infrastructure functionality.

**Database Integration Tests**

SQL Server integration tests require the following environment variable:

```text
SAFEROUTE_TEST_CONNECTION_STRING
```

Configure this variable with a valid SQL Server connection string before running tests that require database access.

## Git Branches

| Branch | Description |
|---|---|
| `main` | Main project branch. |
| `backend-application` | Backend development and integration branch. |

## Project Status

**Status: Under Active Development**

The SafeRouteAI backend is under active development. API functionality, database integration, route processing, authentication, and risk assessment are being developed and tested.

As development progresses, backend components will be integrated and tested together to ensure compatibility and reliability across the complete SafeRouteAI system.

---

*SafeRouteAI – Intelligent Route Safety and Risk Assessment System.*
