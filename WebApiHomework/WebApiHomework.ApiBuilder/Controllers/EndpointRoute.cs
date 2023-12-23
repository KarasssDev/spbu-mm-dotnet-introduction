namespace WebApiHomework.ApiBuilder.Controllers;

public static class EndpointRoute
{
    private const string ApiVersion = "v1";
    private const string ServiceName = $"{ApiVersion}/weather-api";
    public const string SwaggerEndpoint = $"{ServiceName}/swagger";
    public const string WeatherEndpoint = $"{ServiceName}/weather";
    public const string ErrorEndpoint = "/error";
}