using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Web;
using WebApiHomework.Api.Environment;
using WebApiHomework.Core;

namespace WebApiHomework.Api.Integrations;

public class TomorrowIoIntegration: IWeatherServiceIntegration
{

    private static readonly string Credential = EnvironmentCredentialProvider.GetTomorrowIoCredential();

    public async Task<WeatherReport?> GetReport(RequestWeatherData requestWeatherData, CancellationToken token)
    {
        
        var jsonDoc = await JsonHttpClient.PerformGetRequest(
            uriString: "https://api.tomorrow.io/v4/weather/realtime",
            parameters: new NameValueCollection
            {
                ["location"] = $"{requestWeatherData.Latitude.ToString(CultureInfo.InvariantCulture)}," +
                               $"{requestWeatherData.Longitude.ToString(CultureInfo.InvariantCulture)}",
                ["units"] = "metric",
                ["apikey"] = Credential
            },
            token
        );

        var values = jsonDoc.RootElement.GetProperty("data").GetProperty("values");

        return new WeatherReport
        {
            TemperatureCelsius = values.GetDoubleProperty("temperature"),
            Cloudy = values.GetDoubleProperty("cloudCover"),
            Humidity = values.GetDoubleProperty("humidity"),
            Precipitation = values.GetDoubleProperty("rainIntensity"),
            WindDirection = values.GetDoubleProperty("windDirection"),
            WindSpeed = values.GetDoubleProperty("windSpeed")
        };
    }

    public string Name => "tomorrow.io";
}