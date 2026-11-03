namespace SafeRouteAI.Domain.Entities;

public class WeatherLog
{
    public int WeatherLogId { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public DateTime Time { get; set; }
    public double Precipitation { get; set; }
    public double Visibility { get; set; }
    public double WindSpeed { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public string Condition { get; set; } = "";
}