namespace WebApiHomework.Api.Environment;

public static class EnvironmentCredentialProvider
{
    public static string GetTomorrowIoCredential()
    {
        return EnvironmentExtension.GetString("TOMORROW_IO_CREDENTIAL");
    }

    public static string GetOpenWeatherMapCredential()
    {
        return EnvironmentExtension.GetString("OPEN_WEATHER_MAP_CREDENTIAL");
    }
}