using Microsoft.AspNetCore.Mvc;
using WebApiHomework.ApiBuilder.Responses;
using WebApiHomework.ApiBuilder.Settings;
using WebApiHomework.Core;

namespace WebApiHomework.ApiBuilder.Controllers;

[ApiController]
[Route(EndpointRoute.WeatherEndpoint)]
public class WeatherController
{
    private readonly WeatherService _weatherService;
    private readonly IWeatherResponseRenderer _renderer;
    private readonly IApiSettingsProvider _settingsProvider;

    public WeatherController(
        IWeatherServiceIntegrationFactory factory, 
        IWeatherResponseRenderer renderer, 
        IApiSettingsProvider settingsProvider)
    {
        _weatherService = new WeatherService(factory);
        _renderer = renderer;
        _settingsProvider = settingsProvider;
    }

    [HttpGet("{name}")]
    [ProducesResponseType(200, Type = typeof(WeatherResponse))]
    [ProducesResponseType(404, Type = typeof(ErrorResponse))]
    [ProducesResponseType(500, Type = typeof(ErrorResponse))]
    [ProducesResponseType(503, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetWeather(string name, [FromQuery] RequestWeatherData data)
    {
        var timeout = _settingsProvider.GetSettings().RequestTimeout;
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeout);

        if (!_weatherService.IsServiceAvailable(name))
        {
            return new ErrorResponse
            {
                Code = 404,
                Message = $"Unknown service {name}"
            }.ToActionResult();
        }

        try
        {
            var weatherReport = await _weatherService.GetWeatherReport(name, data, cts.Token);

            if (weatherReport == null)
            {
                return new ErrorResponse
                {
                    Code = 503,
                    Message = $"Service {name} unavailable"
                }.ToActionResult();
            }

            return _renderer.RenderResponse(weatherReport).ToActionResult();
        }
        catch (OperationCanceledException)
        {
            return new ErrorResponse
            {
                Code = 503,
                Message = "Request timeout"
            }.ToActionResult();
        }
    }

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(Dictionary<string, WeatherResponse>))]
    [ProducesResponseType(500, Type = typeof(ErrorResponse))]
    [ProducesResponseType(503, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetWeather([FromQuery] RequestWeatherData data)
    {
        var timeout = _settingsProvider.GetSettings().RequestTimeout;
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeout);
        try
        {
            var weatherReports = _weatherService.GetWeatherReports(data, cts.Token);
            var results = new Dictionary<string, WeatherResponse>();

            await foreach (var (name, weatherReport) in weatherReports.WithCancellation(cts.Token))
            {
                if (weatherReport == null)
                {
                    return new ErrorResponse
                    {
                        Code = 503,
                        Message = $"Service {name} unavailable"
                    }.ToActionResult();
                }
                results.Add(name, _renderer.RenderResponse(weatherReport));
            }

            return new ObjectResult(results) { StatusCode = 200 };
        }
        catch (OperationCanceledException)
        {
            return new ErrorResponse
            {
                Code = 503,
                Message = "Request timeout"
            }.ToActionResult();
        }
    }
    
    [HttpGet("integrations")]
    [ProducesResponseType(200, Type = typeof(List<string>))]
    [ProducesResponseType(500, Type = typeof(ErrorResponse))]
    public IActionResult GetServices()
    {
        return new ObjectResult(_weatherService.GetAvailableServices().ToList())
        {
            StatusCode = 200
        };
    }
}