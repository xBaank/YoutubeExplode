using System.Net.Http;
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

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient(HttpClient http, CookiesSettings? cookies = null)
    {
        var youtubeHttp = new HttpClient(new YoutubeHttpMessageHandler(http, cookies), true);

        Videos = new VideoClient(youtubeHttp);
        Playlists = new PlaylistClient(youtubeHttp);
        Channels = new ChannelClient(youtubeHttp);
        Search = new SearchClient(youtubeHttp);
    }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient(CookiesSettings? cookies = null ) : this(Http.Client, cookies)
    {
    }
}