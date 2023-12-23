using WebApiHomework.Api.Integrations;
using WebApiHomework.Core;

namespace WebApiHomework.Api;

public class IntegrationFactory: IWeatherServiceIntegrationFactory
{
    private readonly List<IWeatherServiceIntegration> _integrations = new List<IWeatherServiceIntegration>
    {
        new TomorrowIoIntegration(),
        new OpenWeatherMapIntegration()
    };

    public IWeatherServiceIntegration? GetWeatherServiceIntegration(string name)
    {
        return _integrations.FirstOrDefault(integration => integration.Name == name);
    }

    public IEnumerable<string> AvailableServices => _integrations.Select(x => x.Name);
}