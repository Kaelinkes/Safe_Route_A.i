# SafeRoute AI - API Integration

## Overview

The SafeRoute AI API is a RESTful interface built with ASP.NET Core and .NET 10. It provides a communication layer between the SafeRoute AI frontend, backend services, database, external services, and machine learning components.

The API provides standardized endpoints for authentication, route comparison, road risk assessment, weather information, and community safety reporting.

It is designed to support secure communication, consistent data exchange, input validation, and reliable access to application services.

## Technologies Used

- C#
- ASP.NET Core Web API
- .NET 10
- RESTful API architecture
- SQL Server
- Entity Framework Core
- OpenAPI
- xUnit

## API Functionality

### Authentication and Authorization

The API provides secure access to application functionality through authentication and role-based authorization.

Key functionality includes:

- User registration and login
- Bearer token authentication
- Role-based access control
- Protected API endpoints
- Administrator authorization

Authentication ensures that protected operations are accessible only to authorized users.

### Route Comparison

The route comparison API processes requests containing origin and destination coordinates.

It communicates with routing services to retrieve and return route information, including:

- Route identifiers
- Travel distances
- Estimated travel durations
- Route geometry
- Road risk information

Responses are structured to allow the frontend to display and compare available routes.

### Road Risk Prediction

The risk prediction API provides a standardized interface for requesting road safety assessments.

It accepts road segment information and relevant prediction features, communicates with the risk prediction service, and returns structured risk assessment results.

The API response supports:

- Road segment identification
- Risk classification
- Numerical risk scores
- Prediction model information

This provides a consistent interface between the application and its machine learning services.

### Community Safety Reports

The API supports community-generated road safety reports.

Authenticated users can submit reports about road conditions and safety concerns.

Administrators can review and moderate submitted reports, while approved reports are made available through the application's reporting endpoints.

Role-based authorization protects administrative operations.

### Weather Integration

The API integrates weather information into the application's services.

Weather data is retrieved through external service integration and exposed through structured API responses.

This allows the application to incorporate weather-related information into road safety functionality.

### Input Validation and Error Handling

The API validates incoming requests to maintain data integrity and predictable application behavior.

Validation includes:

- Required request fields
- Valid coordinate values
- Origin and destination validation
- Required prediction features
- Request data consistency

Invalid requests return appropriate HTTP status codes and structured error responses.

## Core API Endpoints

### Health Check

`GET /api/v1/health`

Checks API availability and returns an HTTP success response when the service is running.

### Risk Prediction

`POST /api/v1/risk/predict`

Accepts a road segment identifier and prediction features.

**Example request:**

```json
{
  "segmentId": "segment-001",
  "features": {
    "trafficVolume": 750,
    "rainfallMm": 12,
    "speedLimitKmh": 60
  }
}
```

Returns a structured risk prediction response containing the segment identifier, risk classification, risk score, and model information.

### Route Comparison

`POST /api/v1/routes/compare`

Accepts origin and destination coordinates.

**Example request:**

```json
{
  "origin": {
    "latitude": -26.2041,
    "longitude": 28.0473
  },
  "destination": {
    "latitude": -26.1367,
    "longitude": 28.1976
  }
}
```

Returns route comparison information containing available routes, distances, estimated durations, and associated safety information.

## API Architecture

The API follows a layered architecture that separates HTTP communication from application logic and infrastructure services.

- **Controllers** handle incoming HTTP requests, validate inputs, and return appropriate responses.
- **Data Transfer Objects (DTOs)** define the structure of requests and responses exchanged between the frontend and API.
- **Service Interfaces** establish contracts for application functionality and external integrations.
- **Application Services** coordinate business operations and process requests.
- **Infrastructure Services** provide access to database operations, routing providers, weather services, and other external dependencies.

This separation supports maintainability, testing, and extensibility.

## Installation and Setup

### Prerequisites

- .NET 10 SDK
- SQL Server
- Git
- Required application configuration and external service credentials

### Clone the Repository

```bash
git clone https://github.com/Kaelinkes/Safe_Route_A.i.git
cd Safe_Route_A.i
```

Navigate to the directory containing `SafeRouteAI.slnx`.

### Restore Dependencies

```bash
dotnet restore
```

### Build the Solution

```bash
dotnet build SafeRouteAI.slnx
```

### Run the API

```bash
dotnet run --project SafeRouteAI.API --urls http://localhost:5080
```

The API is accessible at:

`http://localhost:5080`

Configure the database connection and required external service settings before running functionality that depends on them.

## Testing

The API includes automated tests for validating endpoint behavior, request handling, and API contracts.

Run the solution tests using:

```bash
dotnet test SafeRouteAI.slnx
```

The tests help verify request validation, expected HTTP responses, and integration behavior.

## HTTP Responses

The API uses standard HTTP status codes to communicate request outcomes.

- **200 OK** - Request completed successfully.
- **201 Created** - Resource created successfully.
- **204 No Content** - Request completed without a response body.
- **400 Bad Request** - Invalid request data.
- **401 Unauthorized** - Authentication is required or invalid.
- **403 Forbidden** - Insufficient permissions.
- **404 Not Found** - Requested resource could not be found.
- **500 Internal Server Error** - Unexpected server error.

## Security

The API implements authentication and authorization controls to protect application functionality.

Security measures include bearer token authentication, role-based endpoint restrictions, request validation, and controlled access to administrative operations.

Sensitive configuration values and credentials should be managed securely and excluded from source control.

## API Integration Responsibilities

The API layer provides the communication contracts and endpoints used by the SafeRoute AI application.

Its primary responsibilities are:

- Processing frontend HTTP requests
- Connecting application services
- Exposing backend functionality through REST endpoints
- Validating incoming data
- Enforcing authentication and authorization
- Returning standardized responses
- Integrating external service data
- Providing access to road risk prediction functionality

## Developed By

**Ban Kanku**

- **Role:** API Integration Developer
- **GitHub:** https://github.com/BanKanku
- **Project:** SafeRoute AI — IT Project 700
- **Repository:** [SafeRoute AI GitHub Repository](https://github.com/Kaelinkes/Safe_Route_A.i)
- **Branch:** `feature/ml-integration-package`

*This section acknowledges the contribution of Ban Kanku, who was responsible for developing and integrating the API layer of the SafeRoute AI system.*