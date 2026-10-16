namespace PhoneBrowser.Desktop.Extensions;

using System.Net.Http;

internal static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage WithBearerToken(
        this HttpRequestMessage request,
        string token)
    {
        request.Headers.Authorization = new("Bearer", token);

        return request;
    }
}

