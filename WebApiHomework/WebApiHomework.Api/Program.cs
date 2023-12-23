using Microsoft.Extensions.DependencyInjection;
using WebApiHomework.Api;
using WebApiHomework.Api.Environment;
using WebApiHomework.ApiBuilder;
using WebApiHomework.ApiBuilder.Responses;
using WebApiHomework.ApiBuilder.Settings;
using WebApiHomework.Core;

var builder = new WeatherApiBuilder(args);
builder
    .SetupDependency(services =>
    {
        services.AddSingleton<IApiSettingsProvider>(new EnvironmentSettingsProvider());
        services.AddSingleton<IWeatherResponseRenderer>(new WeatherRenderer());
        services.AddSingleton<IWeatherServiceIntegrationFactory>(new IntegrationFactory());
    })
    .BuildApplication()
    .Run();
