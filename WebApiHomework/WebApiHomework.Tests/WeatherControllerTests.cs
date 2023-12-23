using Microsoft.AspNetCore.Mvc;
using WebApiHomework.Api;
using WebApiHomework.ApiBuilder.Controllers;
using WebApiHomework.ApiBuilder.Responses;
using WebApiHomework.ApiBuilder.Settings;
using WebApiHomework.Core;
using Xunit;

namespace WebApiHomework.Tests;


public class WeatherServiceIntegrationMock: IWeatherServiceIntegration
{

    private readonly Queue<WeatherReport?> _reports;
    
    public WeatherServiceIntegrationMock(string name, Queue<WeatherReport?> reports)
    {
        Name = name;
        _reports = reports;
    }

    public Task<WeatherReport?> GetReport(RequestWeatherData requestWeatherData, CancellationToken token)
    {
        return Task.FromResult(_reports.Dequeue());
    }

    public string Name { get; }
}

public class WeatherServiceIntegrationFactoryMock: IWeatherServiceIntegrationFactory
{

    private readonly List<IWeatherServiceIntegration> _integrations;

    public WeatherServiceIntegrationFactoryMock(WeatherReport? service1Reply = null, WeatherReport? service2Reply = null)
    {
        _integrations = new List<IWeatherServiceIntegration>
        {
            new WeatherServiceIntegrationMock("service1", new Queue<WeatherReport?>(new []
            {
                service1Reply
            })),
            new WeatherServiceIntegrationMock("service2", new Queue<WeatherReport?>(new []
            {
                service2Reply
            })),
        };
    }

    public IWeatherServiceIntegration? GetWeatherServiceIntegration(string name)
    {
        return _integrations.FirstOrDefault(integration => integration.Name == name);
    }

    public IEnumerable<string> AvailableServices => _integrations.Select(x => x.Name);
}

public class ApiSettingsProviderMock : IApiSettingsProvider
{
    public ApiSettings GetSettings()
    {
        return new ApiSettings
        {
            RequestTimeout = 3000
        };
    }
}

public class WeatherControllerTests
{

    private readonly RequestWeatherData _requestWeatherData = new RequestWeatherData
    {
        Longitude = 10,
        Latitude = 10
    };

    private readonly WeatherReport _weatherReportSample = new WeatherReport
    {
        TemperatureCelsius = 32,
        Cloudy = 15,
        Humidity = 60,
        Precipitation = 20,
        WindDirection = 120,
        WindSpeed = 23
    };

    private readonly WeatherResponse _weatherReportSampleRendered = new WeatherResponse
    {
        TemperatureCelsius = "32",
        TemperatureFahrenheit = "64",
        Cloudy = "15",
        Humidity = "60",
        Precipitation = "20",
        WindDirection = "120",
        WindSpeed = "23"
    };

    private readonly WeatherReport _weatherReportSampleWithNulls = new WeatherReport
    {
        TemperatureCelsius = 32,
        Cloudy = 15,
        Humidity = null,
        Precipitation = 20,
        WindDirection = null,
        WindSpeed = 23
    };

    private readonly WeatherResponse _weatherReportSampleWithNullsRendered = new WeatherResponse
    {
        TemperatureCelsius = "32",
        TemperatureFahrenheit = "64",
        Cloudy = "15",
        Humidity = "Нет данных",
        Precipitation = "20",
        WindDirection = "Нет данных",
        WindSpeed = "23"
    };

    private static WeatherController CreateController(WeatherReport? service1Reply = null, WeatherReport? service2Reply = null)
    {
        return new WeatherController(
            new WeatherServiceIntegrationFactoryMock(service1Reply, service2Reply), 
            new WeatherRenderer(), 
            new ApiSettingsProviderMock()
        );
    }

    [Fact]
    public void CorrectServiceNames()
    {
        var controller = CreateController();

        var result = controller.GetServices();

        var objectResult = Assert.IsType<ObjectResult>(result);
        var listResult = Assert.IsAssignableFrom<List<string>>(objectResult.Value);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal(new []{ "service1", "service2"}, listResult);
    }
    
    [Fact]
    public async void CorrectServiceReply()
    {
        var controller = CreateController(_weatherReportSample);

        var result = await controller.GetWeather("service1", _requestWeatherData);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var weatherResult = Assert.IsAssignableFrom<WeatherResponse>(objectResult.Value);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal(_weatherReportSampleRendered, weatherResult);
    }

    [Fact]
    public async void CorrectServiceReplyWithNulls()
    {
        var controller = CreateController(_weatherReportSampleWithNulls);

        var result = await controller.GetWeather("service1", _requestWeatherData);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var weatherResult = Assert.IsAssignableFrom<WeatherResponse>(objectResult.Value);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal(_weatherReportSampleWithNullsRendered, weatherResult);
    }

    [Fact]
    public async void CorrectServicesReply()
    {
        var controller = CreateController(_weatherReportSample, _weatherReportSampleWithNulls);

        var result = await controller.GetWeather(_requestWeatherData);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var weatherResults = Assert.IsAssignableFrom<Dictionary<string, WeatherResponse>>(objectResult.Value);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Equal(new Dictionary<string, WeatherResponse>
        {
            ["service1"] = _weatherReportSampleRendered,
            ["service2"] = _weatherReportSampleWithNullsRendered,
        }, weatherResults);
    }

    [Fact]
    public async void ServiceNotFound()
    {
        var controller = CreateController();

        var result = await controller.GetWeather("service3", _requestWeatherData);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.IsAssignableFrom<ErrorResponse>(objectResult.Value);
        Assert.Equal(404, objectResult.StatusCode);
    }
    
    [Fact]
    public async void ServiceUnavailable()
    {
        var controller = CreateController();

        var result = await controller.GetWeather("service1", _requestWeatherData);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.IsAssignableFrom<ErrorResponse>(objectResult.Value);
        Assert.Equal(503, objectResult.StatusCode);
    }
}