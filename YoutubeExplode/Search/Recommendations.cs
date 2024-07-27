using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Search;

/// <summary>
/// Youtube music recommendation
/// </summary>
/// <param name="title"></param>
/// <param name="playlistId"></param>
/// <param name="videoId"></param>
/// <param name="authors"></param>
public class Recommendation(string title, string authors, VideoId? videoId, PlaylistId playlistId)
    : IBatchItem
{
    /// <summary>
    /// Recommendation title
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Contains playlist authors metadata
    /// </summary>
    public string Authors { get; } = authors;

    /// <summary>
    /// First video ID
    /// </summary>
    public VideoId? VideoId { get; } = videoId;

    /// <summary>
    /// Playlist ID
    /// </summary>
    public PlaylistId PlaylistId { get; } = playlistId;
}
