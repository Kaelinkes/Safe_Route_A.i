# SafeRoute AI
### AI-Powered Accident Risk Prediction and Safe Route Recommendation System

![Project](https://img.shields.io/badge/Project-IT%20Project%20700-blue)
![Status](https://img.shields.io/badge/Status-In%20Development-orange)
![Framework](https://img.shields.io/badge/Framework-ASP.NET%20Core-purple)
![Language](https://img.shields.io/badge/Language-C%23%20%7C%20Python-blue)
![Database](https://img.shields.io/badge/Database-SQL%20Server-red)

## 1. Project Overview

SafeRoute AI is an AI-powered, web-based road safety and route recommendation system developed by Group 10 as part of the IT Project 700 module.

The system aims to help drivers make safer and more informed travel decisions by evaluating potential accident risks along different routes.

Unlike conventional navigation applications that primarily focus on travel distance, estimated travel time and traffic conditions, SafeRoute AI introduces an additional safety dimension by analysing:

- Historical road accident data.
- Current weather conditions.
- Community-submitted road incident reports.
- Individual road segment characteristics.
- Predicted accident-risk levels.

Using these sources of information, SafeRoute AI calculates relative accident-risk scores and allows users to compare alternative routes based on both convenience and predicted safety.

The primary goal is to provide drivers with additional information that can help them make better-informed decisions before starting their journeys.

## 2. Problem Statement

Most conventional navigation applications focus primarily on finding the fastest or shortest route.

Although applications such as Google Maps and Waze provide traffic updates and community incident reporting, drivers may not have access to a consolidated assessment of the historical and environmental accident risks associated with their selected routes.

SafeRoute AI aims to address this gap by combining historical accident information, current weather conditions and community incident reports into a unified route-risk assessment system.

The application is particularly intended to support drivers in the South African context, including areas where road conditions, weather and local hazards can vary significantly.

## 3. Project Objectives

The main objectives of SafeRoute AI are to:

1. Develop a web-based route-planning and road safety application.
2. Allow users to enter their starting location and destination.
3. Display alternative routes using map visualisation.
4. Analyse historical accident data to identify accident trends and hotspots.
5. Develop a machine-learning model to predict relative accident risk.
6. Incorporate current weather information into risk assessments.
7. Allow users to submit and view community incident reports.
8. Display accident-risk scores for road segments and complete routes.
9. Compare alternative routes using travel time, distance and predicted risk.
10. Provide an administrative interface for managing community reports and viewing system insights.
11. Implement appropriate security, validation and software quality measures.

## 4. Core Features

### 4.1 User Account Management
- User registration and login.
- Secure password handling.
- User authentication.
- Role-based access control.

### 4.2 Route Planning
- Enter an origin and destination.
- Retrieve alternative routes.
- Display routes using interactive maps.
- View estimated travel time and distance.

### 4.3 Accident Risk Prediction
- Analyse historical accident records.
- Evaluate relevant road segments.
- Incorporate current weather information.
- Consider approved community incident reports.
- Generate relative accident-risk assessments.

### 4.4 Route Risk Comparison
- Compare alternative routes.
- Display risk scores.
- Classify routes into Low, Medium and High risk categories.
- Rank routes using travel time and predicted risk.
- Display explanations for the factors contributing to risk assessments.

### 4.5 Community Incident Reporting
- Submit road hazard reports.
- Include incident descriptions and locations.
- View relevant community-submitted reports.
- Allow administrators to review and moderate submissions.

### 4.6 Administration
- Manage community incident reports.
- Approve or reject submitted reports.
- View aggregated system insights.
- Access administrative functionality through role-based permissions.

## 5. Technology Stack

The following technologies have been selected for the development of SafeRoute AI.

| Technology | Purpose |
|---|---|
| C# | Backend programming |
| ASP.NET Core MVC | Web application framework |
| SQL Server | Relational database |
| Python | Machine learning and data processing |
| Scikit-learn | Machine learning |
| XGBoost | Predictive modelling |
| HTML5 | Web page structure |
| CSS3 | Interface styling |
| JavaScript | Client-side functionality and map visualisation |
| OpenStreetMap | Mapping and geographical data |
| OpenRouteService | Route calculation and directions |
| Open-Meteo | Weather information |
| Git | Version control |
| GitHub | Repository hosting and collaboration |
| Visual Studio 2022 | Development environment |

## 6. System Architecture

SafeRoute AI follows a layered software architecture that separates the presentation, application logic, machine-learning, database and external-service components.

### Architecture Layers

**1. Presentation Layer**
- ASP.NET Core MVC views.
- HTML5 and CSS3.
- JavaScript.
- User interaction and map visualisation.

**2. Application Layer**
- C# application logic.
- User account management.
- Route request processing.
- Input validation.
- Risk assessment coordination.
- Route comparison.

**3. Machine Learning Layer**
- Python-based processing.
- Historical accident data analysis.
- Accident-risk prediction.
- Scikit-learn/XGBoost model integration.

**4. Database Layer**
- Microsoft SQL Server.
- User information.
- Route requests.
- Road segments.
- Accident records.
- Weather information.
- Community reports.
- Risk assessments.

**5. External Services Layer**
- OpenRouteService.
- OpenStreetMap.
- Open-Meteo.

### General System Workflow

1. A registered user enters an origin and destination.
2. The application validates the submitted locations.
3. OpenRouteService retrieves candidate routes.
4. The system identifies the relevant road segments.
5. Historical accident information is retrieved.
6. Current weather information is obtained.
7. Approved community incident reports are considered.
8. The Python machine-learning component evaluates the available information.
9. Risk assessments are generated for eligible road segments.
10. The application calculates route-level risk results.
11. Alternative routes are compared and ranked.
12. The results are displayed to the user.

**Important:** If the required information is insufficient to produce a meaningful assessment, the system must return an `InsufficientData` status rather than presenting an unsupported risk score.

## 7. Database Design

The proposed relational database uses Microsoft SQL Server.

The main database tables include:

| Table | Purpose |
|---|---|
| User | Stores registered users and administrator information. |
| Route | Stores route requests and their associated users. |
| RouteOption | Stores alternative routes. |
| RoadSegment | Stores reusable road-segment information. |
| RouteOptionRoadSegment | Links candidate routes to road segments. |
| AccidentRecord | Stores historical accident information. |
| CommunityReport | Stores user-submitted incident reports. |
| WeatherLog | Stores weather information used in risk assessments. |
| RiskAssessment | Stores segment-level risk assessment results. |
| RiskExplanation | Stores explanations for risk assessments. |
| RouteRiskLog | Stores route comparison results. |
| ModelVersion | Maintains machine-learning model version information. |

The database design incorporates primary keys, foreign keys, unique constraints, validation and appropriate indexing to maintain data integrity.

## 8. Risk Classification

SafeRoute AI uses three main risk classifications:

| Risk Level | Description |
|---|---|
| Low | The available information indicates a relatively lower accident risk. |
| Medium | The available information indicates a moderate accident risk. |
| High | The available information indicates a relatively higher accident risk. |

The system also supports an additional assessment status:

**InsufficientData:** Used when the required information is unavailable or inadequate to produce a meaningful risk assessment.

Risk classifications and prediction thresholds will be determined through model training and validation.

## 9. Security

Security is an important part of the SafeRoute AI development process.

The proposed security measures include:

- Secure user authentication.
- Password hashing.
- Role-based access control.
- Input validation.
- Protection against common web vulnerabilities.
- Secure communication using HTTPS.
- Controlled access to external API credentials.
- Database backup procedures.
- Validation of administrator permissions.

Administrative functionality must only be accessible to authorised users.

## 10. Project Development Methodology

SafeRoute AI follows the Agile Software Development Life Cycle (SDLC).

The project is organised into iterative development stages involving requirements gathering, analysis, design, implementation, testing and documentation.

The development is divided into the following project phases:

| Phase | Description |
|---|---|
| Phase 1 | Project Proposal |
| Phase 2 | Planning Phase |
| Phase 3 | Analysis Phase |
| Phase 4 | System Design |
| Phase 5 | Implementation |
| Phase 6 | Testing and Finalisation |

Each phase builds upon the work completed in the previous phase to ensure that the final software solution remains aligned with the original project objectives.

## 11. Installation and Setup

The following is the intended development environment setup. Exact project commands and configuration names should be updated to match the final repository implementation.

### Prerequisites

Install the following development tools:

- Visual Studio 2022.
- Compatible .NET SDK.
- Microsoft SQL Server or SQL Server Express.
- SQL Server Management Studio.
- Python 3.x.
- Git.
- A modern web browser.

### Clone the Repository

```bash
git clone <repository-url>
```

Navigate into the project directory:

```bash
cd SafeRoute-AI
```

### Configure the Database

1. Install and configure SQL Server.
2. Create the SafeRoute AI database.
3. Configure the database connection in the ASP.NET Core application.
4. Apply the project's database migrations or schema scripts.
5. Verify database connectivity.

### Configure External APIs

The application uses the following external services:

- OpenRouteService: https://openrouteservice.org/
- OpenStreetMap: https://www.openstreetmap.org/
- Open-Meteo: https://open-meteo.com/

Configure any required API credentials using secure configuration or environment variables.

Do not commit API keys, passwords or other sensitive credentials to GitHub.

### Configure Python

Create a Python virtual environment:

```bash
python -m venv venv
```

Activate the environment on Windows:

```bash
venv\Scripts\activate
```

Install the required Python dependencies using the project's dependency file:

```bash
pip install -r requirements.txt
```

### Run the Web Application

Once the application and its configuration are ready:

```bash
dotnet restore
dotnet build
dotnet run
```

The application will display its local development address in the terminal.

Note: The Python prediction component, database and external services must also be configured and running where required.

## 12. Project Scope

### Included in the Project

- Web-based route planning.
- User registration and authentication.
- Alternative route retrieval.
- Historical accident data analysis.
- Machine-learning-based accident-risk prediction.
- Weather integration.
- Community incident reporting.
- Route-risk comparison.
- Administrative management.
- System security and testing.

### Outside the Current Project Scope

The following features are identified as possible future enhancements and are not part of the current academic project:

- Native Android application.
- Native iOS application.
- Continuous GPS vehicle tracking.
- Direct integration with government accident-reporting systems.
- Payment and monetisation functionality.
- Commercial licensing.

## 13. Future Enhancements

Potential future improvements include:

- Development of an Android application.
- Integration of additional official South African accident datasets.
- Live traffic monitoring.
- More advanced weather analysis.
- Improved machine-learning models.
- Additional geographical coverage.
- Enhanced reporting and analytics.

## 14. Repository Structure

The following is a suggested repository organisation. The actual structure should reflect the implementation as development progresses.

```text
SafeRoute-AI/
│
├── SafeRouteAI/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Services/
│   ├── Data/
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── images/
│   ├── appsettings.json
│   └── Program.cs
│
├── MachineLearning/
│   ├── data/
│   ├── models/
│   ├── notebooks/
│   ├── src/
│   └── requirements.txt
│
├── Database/
│   ├── Scripts/
│   ├── Migrations/
│   └── Documentation/
│
├── Documentation/
│   ├── Phase1-Proposal.pdf
│   ├── Phase2-Planning.pdf
│   ├── Phase3-Analysis.pdf
│   └── Phase4-System-Design.pdf
│
├── Tests/
│
├── .gitignore
└── README.md
```

## 15. Academic Project Information

**Module:** IT Project 700

**Institution:** Richfield Graduate Institute of Technology

**Qualification:** Bachelor of Science in Information Technology

**Year:** 2026

**Semester:** 2

**Project Group:** Group 10

**Project Name:** SafeRoute AI

**Project Type:** AI-Powered Web Application

**Development Methodology:** Agile SDLC

## 16. Disclaimer

SafeRoute AI is an academic software development project intended to demonstrate the application of software engineering, database management, web development and machine-learning principles to a real-world road safety problem.

The risk assessments generated by the system are intended to provide additional decision-support information. They should not be interpreted as guarantees of road safety or as replacements for responsible driving decisions.

---

**SafeRoute AI – Making Every Journey a Safer Decision.**
