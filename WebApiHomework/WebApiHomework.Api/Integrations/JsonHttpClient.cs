using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace WebApiHomework.Api.Integrations;

public static class JsonHttpClient
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<JsonDocument> PerformGetRequest(
        string uriString, 
        NameValueCollection parameters, 
        CancellationToken token)
    {
        var realParams = HttpUtility.ParseQueryString(string.Empty);
        realParams.Add(parameters);

        UriBuilder builder = new(uriString)
        {
            Query = realParams.ToString()
        };

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = builder.Uri,
            Headers = { { "accept", "application/json" } },
        };

        string responseBody;
        using (var response = await HttpClient.SendAsync(request, token))
        {
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync(token);
        }

        return JsonDocument.Parse(responseBody);
    }
}