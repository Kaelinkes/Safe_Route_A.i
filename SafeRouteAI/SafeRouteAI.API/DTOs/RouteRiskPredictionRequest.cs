
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SafeRouteAI.API.DTOs;

public sealed class RouteRiskPredictionRequest
{
    [JsonPropertyName("routeId")]
    public string RouteId { get; set; } = string.Empty;

    [JsonPropertyName("segments")]
    public JsonElement Segments { get; set; }
}
