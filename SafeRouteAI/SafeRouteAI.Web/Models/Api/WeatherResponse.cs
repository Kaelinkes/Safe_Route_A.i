namespace SafeRouteAI.Web.Models.Api;

public sealed class WeatherResponse
{
    public required double Latitude { get; init; }

    public required double Longitude { get; init; }

    public required string ObservationTimeUtc { get; init; }

    public required double TemperatureCelsius { get; init; }

    public required double HumidityPercent { get; init; }

    public required double PrecipitationMm { get; init; }

    public required double VisibilityMeters { get; init; }

    public required double WindSpeedKmh { get; init; }

    public required int WeatherCode { get; init; }

    public required bool IsAvailable { get; init; }
}