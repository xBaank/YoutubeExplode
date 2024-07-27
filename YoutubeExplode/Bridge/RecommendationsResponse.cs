using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class RecommendationsResponse(JsonElement content)
{
    [Lazy]
    public JsonElement? ContentRoot =>
        content
            .GetPropertyOrNull("contents")
            ?.GetPropertyOrNull("singleColumnBrowseResultsRenderer")
            ?.GetPropertyOrNull("tabs")
            ?.EnumerateArrayOrEmpty()
            .FirstOrNull()
            ?.GetPropertyOrNull("tabRenderer")
            ?.GetPropertyOrNull("content")
            ?.GetPropertyOrNull("sectionListRenderer");

    [Lazy]
    public RecommendationsData[] Recommendations =>
        ContentRoot
            ?.GetPropertyOrNull("contents")
            ?.EnumerateArrayOrEmpty()
            .Select(i => i.GetPropertyOrNull("musicCarouselShelfRenderer"))
            .WhereNotNull()
            .SelectMany(i =>
                i.GetPropertyOrNull("contents")
                    ?.EnumerateArrayOrEmpty()
                    .Select(i => i.GetPropertyOrNull("musicResponsiveListItemRenderer")) ?? []
            )
            .WhereNotNull()
            .Select(i => new RecommendationsData(i))
            .ToArray() ?? [];

    [Lazy]
    public string? ContinuationToken =>
        ContentRoot
            ?.GetPropertyOrNull("continuations")
            ?.EnumerateArrayOrEmpty()
            .FirstOrNull()
            ?.GetPropertyOrNull("nextContinuationData")
            ?.GetPropertyOrNull("continuation")
            ?.GetStringOrNull();
}

internal partial class RecommendationsData(JsonElement content)
{
    [Lazy]
    private JsonElement? WatchEndpoint =>
        content
            .GetPropertyOrNull("overlay")
            ?.GetPropertyOrNull("musicItemThumbnailOverlayRenderer")
            ?.GetPropertyOrNull("content")
            ?.GetPropertyOrNull("musicPlayButtonRenderer")
            ?.GetPropertyOrNull("playNavigationEndpoint")
            ?.GetPropertyOrNull("watchEndpoint");

    [Lazy]
    public IReadOnlyList<ThumbnailData> Thumbnails =>
        content
            .GetPropertyOrNull("thumbnail")
            ?.GetPropertyOrNull("musicThumbnailRenderer")
            ?.GetPropertyOrNull("thumbnail")
            ?.GetPropertyOrNull("thumbnails")
            ?.EnumerateArrayOrEmpty()
            .Select(i => new ThumbnailData(i))
            .ToList() ?? [];

    [Lazy]
    public string? PlaylistId => WatchEndpoint?.GetPropertyOrNull("playlistId")?.GetStringOrNull();

    [Lazy]
    public string? VideoId => WatchEndpoint?.GetPropertyOrNull("videoId")?.GetStringOrNull();

    [Lazy]
    public string? Title =>
        content
            .GetPropertyOrNull("flexColumns")
            ?.EnumerateArrayOrEmpty()
            .FirstOrNull()
            ?.GetPropertyOrNull("musicResponsiveListItemFlexColumnRenderer")
            ?.GetPropertyOrNull("text")
            ?.GetPropertyOrNull("runs")
            ?.EnumerateArrayOrEmpty()
            .FirstOrNull()
            ?.GetPropertyOrNull("text")
            ?.GetStringOrNull();
}

internal partial class RecommendationsResponse
{
    public static RecommendationsResponse Parse(string content) => new(Json.Parse(content));
}
