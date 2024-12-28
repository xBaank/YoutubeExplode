using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode;

/// <summary>
/// Client for interacting with YouTube.
/// </summary>
public class YoutubeClient
{
    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    ///
    [Obsolete("Use Authenticated static method")]
    public YoutubeClient(HttpClient http, IReadOnlyList<Cookie> initialCookies)
        : this(
            new HttpClient(
                new YoutubeHttpHandler(
                    http,
                    initialCookies,
                    Http.GetDataSyncId(initialCookies).GetAwaiter().GetResult()
                ),
                true
            ),
            true
        ) { }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient(HttpClient http)
        : this(http, false) { }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    private YoutubeClient(HttpClient http, bool isAuthenticated)
    {
        Videos = new VideoClient(http, isAuthenticated);
        Playlists = new PlaylistClient(http);
        Channels = new ChannelClient(http);
        Search = new SearchClient(http);
    }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    [Obsolete("Use Authenticated static method")]
    public YoutubeClient(IReadOnlyList<Cookie> initialCookies)
        : this(Http.Client, initialCookies) { }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient()
        : this(Http.Client) { }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public static async Task<YoutubeClient> WithAuthAsync(
        IReadOnlyList<Cookie> initialCookies,
        HttpClient? http = null
    )
    {
        var youtubeHttp = new HttpClient(
            new YoutubeHttpHandler(
                http ?? Http.Client,
                initialCookies,
                await Http.GetDataSyncId(initialCookies)
            ),
            true
        );

        return new YoutubeClient(youtubeHttp, true);
    }

    /// <summary>
    /// Operations related to YouTube videos.
    /// </summary>
    public VideoClient Videos { get; }

    /// <summary>
    /// Operations related to YouTube playlists.
    /// </summary>
    public PlaylistClient Playlists { get; }

    /// <summary>
    /// Operations related to YouTube channels.
    /// </summary>
    public ChannelClient Channels { get; }

    /// <summary>
    /// Operations related to YouTube search.
    /// </summary>
    public SearchClient Search { get; }
}
