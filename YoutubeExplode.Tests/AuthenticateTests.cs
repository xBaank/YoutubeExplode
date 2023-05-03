using System;
using System.Threading.Tasks;
using Xunit;
using YoutubeExplode.Tests.TestData;

namespace YoutubeExplode.Tests;

public class AuthenticateTest
{
    [Theory]
    [InlineData(VideoIds.AgeRestrictedSexual)]
    [InlineData(VideoIds.AgeRestrictedEmbedRestricted)]
    [InlineData(VideoIds.AgeRestrictedViolent)]
    public async Task Authenticate(string videoId)
    {
        var sapisid = Environment.GetEnvironmentVariable("SAPISID");
        var psid = Environment.GetEnvironmentVariable("PSID");

        if (psid is null || sapisid is null) throw new Exception("SAPISID and PSID must be set as environment variables");

        var cookies = new CookiesSettings(sapisid, psid);
        var client = new YoutubeClient(cookies);
        var video = await client.Videos.Streams.GetManifestAsync(videoId);
        Assert.NotNull(video);
    }
}