using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode;

internal class YoutubeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpClient _http;
    private readonly CookiesSettings? _cookies;

    public YoutubeHttpMessageHandler(HttpClient http, CookiesSettings? cookiesSettings)
    {
        _cookies = cookiesSettings;
        _http = http;
    }

    private async ValueTask<HttpResponseMessage> SendOnceAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // Set API key if necessary
        if (request.RequestUri is not null &&
            request.RequestUri.AbsolutePath.StartsWith("/youtubei/") &&
            !UriEx.ContainsQueryParameter(request.RequestUri.Query, "key"))
        {
            request.RequestUri = new Uri(
                UriEx.SetQueryParameter(
                    request.RequestUri.OriginalString,
                    "key",
                    // This key doesn't appear to change
                    "AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w"
                )
            );
        }

        // Set language if necessary
        if (request.RequestUri is not null && !UriEx.ContainsQueryParameter(request.RequestUri.Query, "hl"))
        {
            request.RequestUri = new Uri(
                UriEx.SetQueryParameter(
                    request.RequestUri.OriginalString,
                    "hl",
                    "en"
                )
            );
        }

        // Set user agent if necessary
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
            );
        }

        // Set required cookies
        if (_cookies is not null)
        {
            const string origin = "https://www.youtube.com";
            request.Headers.Add("Cookie", $"CONSENT=YES+cb; YSC=DwKYllHNwuw; SAPISID={_cookies.Sapisid}; __Secure-3PAPISID={_cookies.Sapisid}; __Secure-3PSID={_cookies.Psid};");
            request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(_cookies.Sapisid, origin)}");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("X-Origin", origin);
            //We can use origin as the referer
            request.Headers.Add("Referer", origin);
        }
        else
        {
            request.Headers.Add("Cookie", "CONSENT=YES+cb; YSC=DwKYllHNwuw");
        }

        var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        // Special case check for rate limiting errors
        if ((int)response.StatusCode == 429)
        {
            throw new RequestLimitExceededException(
                "Exceeded request rate limit. " +
                "Please try again in a few hours. " +
                "Alternatively, inject an instance of HttpClient that includes cookies for a pre-authenticated user."
            );
        }

        return response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        for (var retriesRemaining = 5;; retriesRemaining--)
        {
            try
            {
                using var clonedRequest = request.Clone();
                var response = await SendOnceAsync(clonedRequest, cancellationToken);

                // Retry on 5XX errors
                if ((int)response.StatusCode >= 500 && retriesRemaining > 0)
                {
                    response.Dispose();
                    continue;
                }

                return response;
            }
            // Retry on connectivity issues
            catch (HttpRequestException) when (retriesRemaining > 0)
            {
            }
        }
    }
    
    private static string GenerateSidBasedAuth(string sid, string origin)
    {
        var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var timestamp = date / 1000;
        var sidHash = Hash($"{timestamp} {sid} {origin}");
        return $"{timestamp}_{sidHash}";
    }
    
    static string Hash(string input)
    {
        var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }
}