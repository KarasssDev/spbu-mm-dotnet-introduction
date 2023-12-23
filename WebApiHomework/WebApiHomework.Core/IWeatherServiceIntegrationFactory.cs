namespace WebApiHomework.Core;

public interface IWeatherServiceIntegrationFactory
{
    public IWeatherServiceIntegration? GetWeatherServiceIntegration(string name);
    public IEnumerable<string> AvailableServices { get; }
}