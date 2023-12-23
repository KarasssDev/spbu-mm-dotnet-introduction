using WebApiHomework.ApiBuilder.Responses;
using WebApiHomework.Core;

namespace WebApiHomework.Api;

public class WeatherRenderer: IWeatherResponseRenderer
{
    private static string FromNullable<T>(T? value)
    {
        return (value == null ? "Нет данных" : value.ToString()) ?? throw new InvalidOperationException();
    }

    private static double? CelsiusToFahrenheit(double? value)
    {
        return value * (9 / 5) + 32;
    }

    public WeatherResponse RenderResponse(WeatherReport report)
    {
        return new WeatherResponse
        {
            TemperatureCelsius = FromNullable(report.TemperatureCelsius),
            TemperatureFahrenheit = FromNullable(CelsiusToFahrenheit(report.TemperatureCelsius)),
            Cloudy = FromNullable(report.Cloudy),
            Humidity = FromNullable(report.Humidity),
            WindSpeed = FromNullable(report.WindSpeed),
            WindDirection = FromNullable(report.WindDirection),
            Precipitation = FromNullable(report.Precipitation)
        };
    }
}