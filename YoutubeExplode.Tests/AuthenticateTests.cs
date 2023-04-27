namespace YoutubeExplode.Tests;

using System.Threading.Tasks;
using Xunit;

public class AuthenticateTest
{

    [Theory(Skip = "This test requires manual setup")]
    [InlineData(TestData.VideoIds.AgeRestrictedSexual)]
    [InlineData(TestData.VideoIds.AgeRestrictedEmbedRestricted)]
    [InlineData(TestData.VideoIds.AgeRestrictedViolent)]
    public async Task Authenticate(string videoId)
    {
        var cookies = new CookiesSettings("", "");
        var client = new YoutubeClient(cookies);
        var video = await client.Videos.Streams.GetManifestAsync(videoId);
        Assert.NotNull(video);
    }
}