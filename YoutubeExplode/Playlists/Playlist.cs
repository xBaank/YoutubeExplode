using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Metadata associated with a YouTube playlist.
/// </summary>
public class Playlist : IPlaylist
{
    /// <inheritdoc />
    public PlaylistId Id { get; }

    /// <inheritdoc />
    public string Url => _url ?? $"https://www.youtube.com/playlist?list={Id}";

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public Author? Author { get; }

    /// <summary>
    /// Playlist description.
    /// </summary>
    public string Description { get; }

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; }

    private string? _url;

    /// <summary>
    /// Initializes an instance of <see cref="Playlist"/>.
    /// </summary>
    public Playlist(
        PlaylistId id,
        string title,
        Author? author,
        string description,
        IReadOnlyList<Thumbnail> thumbnails,
        string? url = null)
    {
        Id = id;
        Title = title;
        Author = author;
        Description = description;
        Thumbnails = thumbnails;
        _url = url;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Playlist ({Title})";
}