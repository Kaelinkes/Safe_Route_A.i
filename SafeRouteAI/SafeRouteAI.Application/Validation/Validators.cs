using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Entities;

namespace SafeRouteAI.Application.Validation;

public static class Validators
{
    public static bool IsValid(Coordinate c) =>
        c.Lat is >= -90 and <= 90 && c.Lon is >= -180 and <= 180;

    public static bool IsValid(CommunityReport r) =>
        !string.IsNullOrWhiteSpace(r.IncidentType)
        && (r.Description?.Length ?? 0) <= 500
        && r.Latitude is >= -90 and <= 90 && r.Longitude is >= -180 and <= 180;
}