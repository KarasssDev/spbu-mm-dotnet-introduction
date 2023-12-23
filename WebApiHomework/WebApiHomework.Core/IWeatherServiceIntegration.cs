namespace WebApiHomework.Core;

public interface IWeatherServiceIntegration
{
    public Task<WeatherReport?> GetReport(RequestWeatherData requestWeatherData, CancellationToken token);
    public string Name { get; }
}
