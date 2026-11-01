namespace SafeRouteAI.Domain.Entities;

public class RouteRiskLog
{
    public int RouteRiskLogID { get; set; }

    public int RouteID { get; set; }

    public DateTime LoggedAt { get; set; }

    // Proposed field for storing the route and risk result snapshot
    public string ResultSnapshot { get; set; } = string.Empty;

    // Navigation property
    public Route? Route { get; set; }
}

// Phase 4 does not prescribe its exact column name or data type, so confirm this with the database team. -Kaelin Kesavjee