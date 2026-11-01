# SafeRouteAI.Domain

## 1. Overview

The `SafeRouteAI.Domain` project contains the core entities and business concepts used by the SafeRoute AI system.

It represents the main data structures of the application, including users, routes, road segments, accident records, community reports, weather information and risk assessments.

The Domain layer provides the foundation for the Application and Infrastructure layers.

## 2. Project Structure

```text
SafeRouteAI.Domain/
├── Entities/
│   ├── User.cs
│   ├── Route.cs
│   ├── RouteOption.cs
│   ├── RoadSegment.cs
│   ├── RouteOptionRoadSegment.cs
│   ├── AccidentRecord.cs
│   ├── CommunityReport.cs
│   ├── WeatherLog.cs
│   ├── RiskAssessment.cs
│   ├── RiskExplanation.cs
│   ├── RouteRiskLog.cs
│   └── ModelVersion.cs
└── README.md
```

## 3. Domain Entities

| Entity | Description |
|---|---|
| User | Represents registered system users and their roles. |
| Route | Stores route requests made by users. |
| RouteOption | Represents an alternative route generated for a route request. |
| RoadSegment | Represents an individual road segment used in route analysis. |
| RouteOptionRoadSegment | Links route options to their road segments and records segment order. |
| AccidentRecord | Stores historical accident information. |
| CommunityReport | Stores incident reports submitted by users. |
| WeatherLog | Stores weather observations associated with geographical locations and dates. |
| RiskAssessment | Stores risk assessment results for road segments. |
| RiskExplanation | Stores explanations associated with risk assessments. |
| RouteRiskLog | Stores historical route risk result snapshots. |
| ModelVersion | Stores information about the machine learning model versions. |

## 4. Entity Relationships

The Domain entities represent the following relationships:

- A User can have multiple Routes.
- A Route can have multiple RouteOptions.
- A RouteOption can contain multiple RoadSegments.
- A RoadSegment can belong to multiple RouteOptions through RouteOptionRoadSegment.
- A RoadSegment can have multiple RiskAssessments.
- A WeatherLog can be associated with multiple RiskAssessments.
- A RiskAssessment can have an associated RiskExplanation.
- A User can submit multiple CommunityReports.
- A User with an administrative role can review CommunityReports.
- A Route can have multiple RouteRiskLogs.

## 5. Important Domain Considerations

- Risk scores and risk levels can be nullable when an assessment has insufficient data.
- Risk assessments use the statuses `Valid` and `InsufficientData`.
- Community reports use the statuses `Pending`, `Approved` and `Rejected`.
- Administrative users are represented through the User entity's role.
- Route options are connected to road segments through the linking entity `RouteOptionRoadSegment`.

## 6. Implementation Notes

The entity classes are based on the project's Phase 3 and Phase 4 documentation.

Some implementation details, including coordinate representation, certain field names, and the RouteRiskLog snapshot format, may require alignment with the database design.

These details should be agreed upon with the database team before the database schema and entity configurations are finalised.

## 7. Project Dependencies

The Domain project is intended to remain independent of database and user-interface implementation details.

Other solution projects can reference the Domain project to use its core entities and business concepts.

## 8. Development Credits

### Developed By

**Kaelin Kes**

- **Role:** Team Lead & Backend Developer
- **GitHub:** [@Kaelinkes](https://github.com/Kaelinkes)
- **Repository:** [SafeRouteAI – GitHub Repository](https://github.com/Kaelinkes/Safe_Route_A.i)

The initial Domain layer implementation, including the entity classes and project documentation, was developed by Kaelin Kes as part of the SafeRoute AI IT Project.

*This section acknowledges the contribution of the developer responsible for the initial Domain layer implementation.*