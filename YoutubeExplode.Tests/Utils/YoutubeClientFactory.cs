using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace YoutubeExplode.Tests.Utils;

internal static class YoutubeClientFactory
{
    public static async ValueTask<YoutubeClient> GetYoutubeClientAsync() =>
        Secrets.YoutubeCookiesPath is not null
            ? await YoutubeClient.WithAuthAsync(ParseCookies(Secrets.YoutubeCookiesPath))
            : new YoutubeClient();

    private static List<Cookie> ParseCookies(string path)
    {
        var cookies = new List<Cookie>();

        foreach (var line in File.ReadAllLines(path))
        {
            // Skip comments and empty lines
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                continue;

            // Split the line into parts
            var parts = line.Split('\t');
            if (parts.Length != 7)
                continue; // Invalid format, skip this line

            var cookie = new Cookie
            {
                Name = parts[5],
                Value = parts[6],
                Domain = parts[0],
                Path = parts[2],
                HttpOnly = parts[3] == "TRUE",
                Secure = parts[1] == "TRUE",
                Expires = DateTimeOffset
                    .FromUnixTimeSeconds(
                        long.Parse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture)
                    )
                    .DateTime,
            };

            cookies.Add(cookie);
        }

        return cookies;
    }
}
