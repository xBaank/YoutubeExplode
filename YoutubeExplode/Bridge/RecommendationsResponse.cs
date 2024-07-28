﻿using System;
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
            ?.GetPropertyOrNull("sectionListRenderer")
        ?? content
            .GetPropertyOrNull("continuationContents")
            ?.GetPropertyOrNull("sectionListContinuation");

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
                    .Select(i =>
                        i.GetPropertyOrNull("musicResponsiveListItemRenderer")
                        ?? i.GetPropertyOrNull("musicTwoRowItemRenderer")
                    ) ?? []
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

    [Lazy]
    public string? VisitorData =>
        content
            .GetPropertyOrNull("responseContext")
            ?.GetPropertyOrNull("visitorData")
            ?.GetStringOrNull();
}

internal partial class RecommendationsData(JsonElement content)
{
    [Lazy]
    private JsonElement? NavigationEndpoint =>
        content
            .GetPropertyOrNull("overlay")
            ?.GetPropertyOrNull("musicItemThumbnailOverlayRenderer")
            ?.GetPropertyOrNull("content")
            ?.GetPropertyOrNull("musicPlayButtonRenderer")
            ?.GetPropertyOrNull("playNavigationEndpoint")
        ?? content
            .GetPropertyOrNull("menu")
            ?.GetPropertyOrNull("menuRenderer")
            ?.GetPropertyOrNull("items")
            ?.EnumerateArrayOrEmpty()
            .FirstOrNull()
            ?.GetPropertyOrNull("menuNavigationItemRenderer")
            ?.GetPropertyOrNull("navigationEndpoint");

    [Lazy]
    private JsonElement? WatchEndpoint =>
        NavigationEndpoint?.GetPropertyOrNull("watchPlaylistEndpoint")
        ?? NavigationEndpoint?.GetPropertyOrNull("watchEndpoint");

    [Lazy]
    private JsonElement? ThumbnailsRoot =>
        content.GetPropertyOrNull("thumbnail") ?? content.GetPropertyOrNull("thumbnailRenderer");

    [Lazy]
    public IReadOnlyList<ThumbnailData> Thumbnails =>
        ThumbnailsRoot
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
    public string? Subtitle =>
        content
            .GetPropertyOrNull("flexColumns")
            ?.EnumerateArrayOrEmpty()
            .Skip(1)
            .Select(i =>
                i.GetPropertyOrNull("musicResponsiveListItemFlexColumnRenderer")
                    ?.GetPropertyOrNull("text")
                    ?.GetPropertyOrNull("runs")
                    ?.EnumerateArrayOrEmpty()
                    .FirstOrNull()
                    ?.GetPropertyOrNull("text")
                    ?.GetStringOrNull()
            )
            .WhereNotNull()
            .Pipe(i => string.Join(" - ", i))
        ?? content
            .GetPropertyOrNull("subtitle")
            ?.GetPropertyOrNull("runs")
            ?.EnumerateArrayOrEmpty()
            .Select(i => i.GetPropertyOrNull("text")?.GetStringOrNull())
            .WhereNotNull()
            .Pipe(i => string.Join("", i));

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
            ?.GetStringOrNull()
        ?? content
            .GetPropertyOrNull("title")
            ?.GetPropertyOrNull("runs")
            ?.EnumerateArrayOrNull()
            ?.FirstOrNull()
            ?.GetPropertyOrNull("text")
            ?.GetStringOrNull();
}

internal partial class RecommendationsResponse
{
    public static RecommendationsResponse Parse(string content) => new(Json.Parse(content));
}
