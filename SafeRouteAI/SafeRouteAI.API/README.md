\# SafeRoute AI – API Integration



\## Overview

The API Integration Layer connects the SafeRoute AI Frontend with the Backend services, database functionality, and Machine Learning model.



My responsibility was to develop and test the API endpoints that allow the different components to communicate.



\## Technologies

\- C# / ASP.NET Core Web API

\- .NET 10

\- REST API and JSON

\- Bearer Token Authentication

\- xUnit Testing

\- GitHub



\## Implemented Features

\- User registration and login

\- Authentication and role-based authorization

\- Route comparison API

\- Risk prediction API contract

\- Weather API

\- Community incident reporting

\- Admin report approval and rejection

\- Input validation and error handling

\- API integration and automated testing



\## Main API Endpoints



| Method | Endpoint | Purpose |

|---|---|---|

| GET | `/api/v1/health` | Check API status |

| POST | `/api/v1/auth/register` | Register user |

| POST | `/api/v1/auth/login` | User login |

| POST | `/api/v1/routes/compare` | Compare routes |

| POST | `/api/v1/risk/predict` | Request risk prediction |

| GET | `/api/v1/weather` | Get weather information |

| POST | `/api/v1/reports` | Submit incident report |

| GET | `/api/v1/reports` | View approved reports |

| GET | `/api/v1/admin/reports/pending` | View pending reports |

| PATCH | `/api/v1/admin/reports/{id}/status` | Approve or reject reports |



\## Testing

The API was tested using automated tests and CMD requests.



\*\*Latest solution test results:\*\*

\- 71 total tests

\- 70 passed

\- 0 failed

\- 1 skipped

\- Build successful



Authentication, Community Reports, and admin authorization were successfully tested against the database.



\## Running the API



From the solution directory:



```bash

dotnet run --project SafeRouteAI.API

```



\## Current Status

The API integration for the completed features has been implemented and tested.



\*\*Outstanding:\*\* Connecting the final Python ML model for real risk predictions and completing final end-to-end verification.



\## My Contribution

My work focused specifically on the \*\*API Integration Layer\*\*, including endpoints, DTOs, validation, authentication integration, service connections, and API testing.



The Frontend, Backend business logic, Domain Layer, Database implementation, and Python ML model were handled by other team members.

