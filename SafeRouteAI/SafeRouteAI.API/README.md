SafeRoute AI - API Integration

Overview

SafeRoute AI is a road safety application designed to help users compare routes based on travel information and potential safety risks.

The API integration layer connects the frontend application to backend services, the database, external services, and the risk prediction component.

 API Features

 Authentication and Authorization
- User registration and login.
- Bearer token authentication.
- Role-based access control.
- Protected endpoints for authorized users and administrators.

 Route Comparison
- Accepts origin and destination coordinates.
- Integrates with routing services to retrieve route information.
- Provides route comparison data to the frontend.
- Supports risk information associated with routes.

 Risk Prediction
- Provides an endpoint for road risk predictions.
- Defines request and response contracts for the prediction service.
- Supports integration with the Python machine learning model.
- Uses a mock prediction service while final ML integration is pending.

 Community Reports
- Allows authenticated users to submit road safety reports.
- Supports administrator review and moderation.
- Makes approved reports available through the API.
- Restricts administrative actions to authorized users.

 Weather Integration
- Provides weather-related information through the API.
- Supports the use of weather conditions in road safety features.

 Input Validation and Error Handling
- Validates incoming API requests.
- Rejects invalid or incomplete inputs.
- Returns appropriate HTTP status codes and error responses.

 Main API Endpoints

- `GET /api/v1/health` — Check API availability.
- `POST /api/v1/risk/predict` — Request a risk prediction.
- `POST /api/v1/routes/compare` — Compare routes.

Additional endpoints support authentication, community reports, weather information, and administration.

 Technologies

- ASP.NET Core Web API
- C# and .NET 10
- SQL Server
- Entity Framework Core
- REST API
- OpenAPI
- xUnit

 Running the API

From the solution directory:

```cmd
dotnet restore
dotnet build SafeRouteAI.slnx
dotnet run --project SafeRouteAI.API --urls http://localhost:5080
```

The API runs at:

`http://localhost:5080`

Database connectivity, connection strings, and any required external service credentials must be configured for the relevant features to work.

## Testing

Run the automated tests:

```cmd
dotnet test SafeRouteAI.slnx
```

