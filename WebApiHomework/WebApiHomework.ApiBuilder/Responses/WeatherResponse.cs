using Microsoft.AspNetCore.Mvc;

namespace WebApiHomework.ApiBuilder.Responses;

public record WeatherResponse: IResponse
{
    public required string TemperatureCelsius { get; init; }
    public required string TemperatureFahrenheit { get; init; }
    public required string Cloudy { get; init; }
    public required string Humidity { get; init; }
    public required string WindSpeed { get; init; }
    public required string WindDirection { get; init; }
    public required string Precipitation { get; init; }

    public IActionResult ToActionResult()
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = 200
        };
        return objectResult;
    }
}