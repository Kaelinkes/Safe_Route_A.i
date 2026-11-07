# SafeRouteAI.Web

## 1. Overview

The SafeRouteAI.Web project contains the frontend of the SafeRoute AI system.

It provides the user interface through which users can interact with the application, plan journeys, compare route options and access available road-safety information.

The frontend is developed using ASP.NET Core MVC and Razor, with HTML, CSS and JavaScript for the user interface and interactive functionality.

## 2. Project Structure

text
SafeRouteAI.Web/
├── Controllers/
├── Models/
├── Views/
│   ├── Home/
│   ├── Account/
│   ├── Routes/
│   ├── Reports/
│   ├── Admin/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── Program.cs
└── SafeRouteAI.Web.csproj


## 3. Main Frontend Features

| Feature | Description |
|---|---|
| Homepage | Introduces SafeRoute AI and its main functionality. |
| Login | Provides the user login interface. |
| Registration | Provides the account registration interface. |
| Dashboard | Provides the main user dashboard. |
| Route Planner | Allows users to select journey starting points and destinations. |
| Route Comparison | Displays available route options, distance and estimated travel time. |
| Weather Information | Displays weather information for selected geographical coordinates. |
| Risk Information | Provides an interface for displaying available road-safety information. |
| Community Reports | Provides an interface for submitting road incident reports. |
| Route History | Provides an interface for viewing previous journeys. |
| Admin Moderation | Provides an interface for reviewing community reports. |
| Admin Analytics | Provides an interface for displaying application statistics. |

## 4. Technologies Used

- *C#* – Application programming language.
- *ASP.NET Core MVC* – Web application framework.
- *Razor* – Dynamic HTML rendering.
- *HTML5* – Page structure.
- *CSS3* – Styling and responsive layouts.
- *JavaScript* – Interactive frontend functionality.
- *Leaflet* – Interactive map functionality.
- *OpenStreetMap* – Geographical map data.

## 5. Map Implementation

- Leaflet is used to provide interactive map functionality.
- OpenStreetMap provides geographical map data.
- Users can select starting points and destinations directly on the map.
- Route information can be presented through distance, estimated travel time and available route geometry.

## 6. Important Implementation Considerations

- The frontend is designed to provide a simple and user-friendly experience.
- Insufficient risk data must not be presented as low risk.
- Risk scores and predictions may be unavailable when there is insufficient information.
- Community reports use the statuses Pending, Approved and Rejected.
- Risk information is intended as decision support and not a guarantee of road conditions.

## 7. Project Dependencies

The SafeRouteAI.Web project forms part of the larger SafeRoute AI solution.

It provides the user-facing application and works alongside the other components developed by the project team.

## 8. Development Credits

### Developed By

*Mohamed Aasif Rahman*

- *Role:* Frontend Developer
- *GitHub:* [@MohamedAasifRahman](https://github.com/MohamedAasifRahman)
- *Repository:* [SafeRouteAI – GitHub Repository](https://github.com/Kaelinkes/Safe_Route_A.i)

The frontend implementation, including the user interfaces, navigation, route-planning experience and interactive map functionality, was developed by Mohamed Aasif Rahman as part of the SafeRoute AI IT Project.

This section acknowledges the contribution of the developer responsible for the frontend implementation.
