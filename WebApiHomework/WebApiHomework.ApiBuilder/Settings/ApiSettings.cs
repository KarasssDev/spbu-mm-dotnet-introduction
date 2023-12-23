namespace WebApiHomework.ApiBuilder.Settings;

public record ApiSettings
{
    public required int RequestTimeout { get; init; }
}