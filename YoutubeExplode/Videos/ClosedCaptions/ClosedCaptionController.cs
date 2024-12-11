﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.ClosedCaptions;

internal class ClosedCaptionController(HttpClient http) : VideoController(http)
{
    public async ValueTask<ClosedCaptionTrackResponse> GetClosedCaptionTrackResponseAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        // Enforce known format
        var urlWithFormat = url.Pipe(s => UrlEx.SetQueryParameter(s, "format", "3"))
            .Pipe(s => UrlEx.SetQueryParameter(s, "fmt", "3"));

        if (!urlWithFormat.StartsWith("https://www.youtube.com"))
            urlWithFormat = "https://www.youtube.com" + urlWithFormat;

        return ClosedCaptionTrackResponse.Parse(
            await Http.GetStringAsync(urlWithFormat, cancellationToken)
        );
    }
}
