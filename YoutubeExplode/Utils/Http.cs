using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace YoutubeExplode.Utils;

internal static class Http
{
    private static readonly Lazy<HttpClient> HttpClientLazy = new(() => new HttpClient());

    public static HttpClient Client => HttpClientLazy.Value;

    public static string? GetDataSyncId(IReadOnlyList<Cookie> cookies)
    {
        if (cookies.Count == 0)
            return null;

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

        var result = Http
            .Client.SendAsync(request)
            .GetAwaiter()
            .GetResult()
            .Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();
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
