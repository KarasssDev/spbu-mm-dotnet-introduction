using System.Text.Json;

namespace WebApiHomework.Api.Integrations;

public static class JsonElementExtension
{
    public static double? GetDoubleProperty(this JsonElement element, string name)
    {
        var successful = element.TryGetProperty(name, out var property);
        if (!successful) return null;

        successful = property.TryGetDouble(out var result);
        if (!successful) return null;

        return result;
    }
}