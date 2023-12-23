using WebApiHomework.Core;

namespace WebApiHomework.ApiBuilder.Responses;

public interface IWeatherResponseRenderer
{
    public WeatherResponse RenderResponse(WeatherReport report);
}