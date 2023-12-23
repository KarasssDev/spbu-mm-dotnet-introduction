namespace WebApiHomework.Core;

public class UnknownServiceIntegration: Exception
{
    public UnknownServiceIntegration(string integrationName)
    {
        IntegrationName = integrationName;
    }

    public string IntegrationName { get; }
}