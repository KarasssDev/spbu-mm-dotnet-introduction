using WebApiHomework.ApiBuilder.Settings;

namespace WebApiHomework.Api.Environment;

public class EnvironmentSettingsProvider: IApiSettingsProvider
{
    public ApiSettings GetSettings()
    {
        return new ApiSettings
        {
            RequestTimeout = EnvironmentExtension.GetInt("REQUEST_TIMEOUT")
        };
    }
}