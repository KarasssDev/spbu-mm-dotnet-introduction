using System.Runtime.CompilerServices;

namespace WebApiHomework.Core;

public class WeatherService
{
    private readonly IWeatherServiceIntegrationFactory _factory;

    public WeatherService(IWeatherServiceIntegrationFactory factory)
    {
        _factory = factory;
    }

    public async Task<WeatherReport?> GetWeatherReport(
        string integrationName, 
        RequestWeatherData requestWeatherData, 
        CancellationToken token)
    {
        var serviceIntegration = _factory.GetWeatherServiceIntegration(integrationName);
        if (serviceIntegration == null)
        {
            throw new UnknownServiceIntegration(integrationName);
        }
        return await serviceIntegration.GetReport(requestWeatherData, token);
    }

    public async IAsyncEnumerable<Tuple<string, WeatherReport?>> GetWeatherReports(
        RequestWeatherData requestWeatherData,
        [EnumeratorCancellation] CancellationToken token)
    {
        foreach (var integrationName in _factory.AvailableServices)
        {
            yield return new Tuple<string, WeatherReport?>(
                integrationName, 
                await GetWeatherReport(integrationName, requestWeatherData, token)
            );
        }
    }

    public IEnumerable<string> GetAvailableServices()
    {
        return _factory.AvailableServices;
    }

    public bool IsServiceAvailable(string integrationName)
    {
        return _factory.AvailableServices.Contains(integrationName);
    }
}