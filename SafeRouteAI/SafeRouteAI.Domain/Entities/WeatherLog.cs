namespace SafeRouteAI.Domain.Entities;

public class WeatherLog
{
    public int WeatherLogID { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public DateTime DateTime { get; set; }

    public double? Precipitation { get; set; }

    public double? Humidity { get; set; }

    public double? Visibility { get; set; }

    public double? WindSpeed { get; set; }

    public ICollection<RiskAssessment> RiskAssessments { get; set; }
        = new List<RiskAssessment>();
}