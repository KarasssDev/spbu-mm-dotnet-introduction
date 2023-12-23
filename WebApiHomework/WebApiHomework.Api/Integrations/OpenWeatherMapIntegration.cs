using System.Collections.Specialized;
using System.Globalization;
using WebApiHomework.Api.Environment;
using WebApiHomework.Core;

namespace WebApiHomework.Api.Integrations;

public class OpenWeatherMapIntegration: IWeatherServiceIntegration
{
    private static readonly string Credential = EnvironmentCredentialProvider.GetOpenWeatherMapCredential();

    public async Task<WeatherReport?> GetReport(RequestWeatherData requestWeatherData, CancellationToken token)
    {
        
        var jsonDoc = await JsonHttpClient.PerformGetRequest(
            uriString: "https://api.openweathermap.org/data/2.5/weather",
            parameters: new NameValueCollection
            {
                ["lat"] = requestWeatherData.Latitude.ToString(CultureInfo.InvariantCulture),
                ["lon"] = requestWeatherData.Longitude.ToString(CultureInfo.InvariantCulture),
                ["units"] = "metric",
                ["appid"] = Credential
            },
            token
        );

        var main = jsonDoc.RootElement.GetProperty("main");
        var wind = jsonDoc.RootElement.GetProperty("wind");
        var clouds = jsonDoc.RootElement.GetProperty("clouds");


        return new WeatherReport
        {
            TemperatureCelsius = main.GetDoubleProperty("temp"),
            Cloudy = clouds.GetDoubleProperty("all"),
            Humidity = main.GetDoubleProperty("humidity"),
            Precipitation = jsonDoc.RootElement.GetDoubleProperty("rain"),
            WindDirection = wind.GetDoubleProperty("deg"),
            WindSpeed = wind.GetDoubleProperty("speed")
        };
    }

    public string Name => "openweathermap.org";
}