namespace WebApiHomework.Core;

public record WeatherReport
{
    public double? TemperatureCelsius { get; init; }
    public double? Cloudy { get; init; }
    public double? Humidity { get; init; }
    public double? WindSpeed { get; init; }
    public double? WindDirection { get; init; }
    public double? Precipitation { get; init; }
}