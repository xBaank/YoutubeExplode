using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils;

internal static class Http
{
    private static readonly Lazy<HttpClient> HttpClientLazy = new(() => new HttpClient());

    public static HttpClient Client => HttpClientLazy.Value;

    public static async Task<string?> GetDataSyncId(IReadOnlyList<Cookie> cookies) =>
        await GetDataSyncFromWebPageId(cookies) ?? await GetDataSyncFallbackId(cookies);

    public static async Task<string?> GetDataSyncFromWebPageId(IReadOnlyList<Cookie> cookies)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://www.youtube.com"),
            Method = HttpMethod.Get,
        };

        var cookieContainer = new CookieContainer();
        foreach (var cookie in cookies)
            cookieContainer.Add(cookie);

        if (!request.Headers.Contains("Cookie"))
        {
            var cookieHeaderValue = cookieContainer.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrWhiteSpace(cookieHeaderValue))
                request.Headers.Add("Cookie", cookieHeaderValue);
        }

        var response = await Http.Client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        var regex = new Regex("""datasyncId":"(.*?)\|\|""");
        var groups = regex.Match(result);

        return groups.Success ? groups.Groups[1].Value : null;
    }

    public static async Task<string?> GetDataSyncFallbackId(IReadOnlyList<Cookie> cookies)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://www.youtube.com/getDatasyncIdsEndpoint"),
            Method = HttpMethod.Get,
        };

        var cookieContainer = new CookieContainer();
        foreach (var cookie in cookies)
            cookieContainer.Add(cookie);

        if (!request.Headers.Contains("Cookie"))
        {
            var cookieHeaderValue = cookieContainer.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrWhiteSpace(cookieHeaderValue))
                request.Headers.Add("Cookie", cookieHeaderValue);
        }

        var response = await Http.Client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        var datasyncIds = string.Join("\n", result.Split("\n").Skip(1));
        var dataSyncIdJson = Json.TryParse(datasyncIds);
        var dataSyncId = dataSyncIdJson
            ?.GetProperty("responseContext")
            .GetProperty("mainAppWebResponseContext")
            .GetProperty("datasyncId")
            .GetString()
            ?.Split("||");

        return dataSyncId?.ElementAtOrDefault(0)
            ?? throw new YoutubeExplode.Exceptions.YoutubeExplodeException("Couldn't permorm auth");
    }
}
