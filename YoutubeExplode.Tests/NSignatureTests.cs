using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.NCipher;

namespace YoutubeExplode.Tests;

public class NSignatureTests
{
    public static TheoryData<string, string, string> GetPlayerTestCases()
    {
        return new TheoryData<string, string, string>
        {
            {
                "https://www.youtube.com/s/player/7862ca1f/player_ias.vflset/en_US/base.js",
                "X_LCxVDjAavgE5t",
                "yxJ1dM6iz5ogUg"
            },
            {
                "https://www.youtube.com/s/player/9216d1f7/player_ias.vflset/en_US/base.js",
                "SLp9F5bwjAdhE9F-",
                "gWnb9IK2DJ8Q1w"
            },
            {
                "https://www.youtube.com/s/player/f8cb7a3b/player_ias.vflset/en_US/base.js",
                "oBo2h5euWy6osrUt",
                "ivXHpm7qJjJN"
            },
            {
                "https://www.youtube.com/s/player/2dfe380c/player_ias.vflset/en_US/base.js",
                "oBo2h5euWy6osrUt",
                "3DIBbn3qdQ"
            },
            {
                "https://www.youtube.com/s/player/f1ca6900/player_ias.vflset/en_US/base.js",
                "cu3wyu6LQn2hse",
                "jvxetvmlI9AN9Q"
            },
            {
                "https://www.youtube.com/s/player/8040e515/player_ias.vflset/en_US/base.js",
                "wvOFaY-yjgDuIEg5",
                "HkfBFDHmgw4rsw"
            },
            {
                "https://www.youtube.com/s/player/e06dea74/player_ias.vflset/en_US/base.js",
                "AiuodmaDDYw8d3y4bf",
                "ankd8eza2T6Qmw"
            },
            {
                "https://www.youtube.com/s/player/5dd88d1d/player-plasma-ias-phone-en_US.vflset/base.js",
                "kSxKFLeqzv_ZyHSAt",
                "n8gS8oRlHOxPFA"
            },
            {
                "https://www.youtube.com/s/player/324f67b9/player_ias.vflset/en_US/base.js",
                "xdftNy7dh9QGnhW",
                "22qLGxrmX8F1rA"
            },
            {
                "https://www.youtube.com/s/player/4c3f79c5/player_ias.vflset/en_US/base.js",
                "TDCstCG66tEAO5pR9o",
                "dbxNtZ14c-yWyw"
            },
            {
                "https://www.youtube.com/s/player/c81bbb4a/player_ias.vflset/en_US/base.js",
                "gre3EcLurNY2vqp94",
                "Z9DfGxWP115WTg"
            },
            {
                "https://www.youtube.com/s/player/1f7d5369/player_ias.vflset/en_US/base.js",
                "batNX7sYqIJdkJ",
                "IhOkL_zxbkOZBw"
            },
            {
                "https://www.youtube.com/s/player/009f1d77/player_ias.vflset/en_US/base.js",
                "5dwFHw8aFWQUQtffRq",
                "audescmLUzI3jw"
            },
            {
                "https://www.youtube.com/s/player/dc0c6770/player_ias.vflset/en_US/base.js",
                "5EHDMgYLV6HPGk_Mu-kk",
                "n9lUJLHbxUI0GQ"
            },
            {
                "https://www.youtube.com/s/player/113ca41c/player_ias.vflset/en_US/base.js",
                "cgYl-tlYkhjT7A",
                "hI7BBr2zUgcmMg"
            },
            {
                "https://www.youtube.com/s/player/c57c113c/player_ias.vflset/en_US/base.js",
                "M92UUMHa8PdvPd3wyM",
                "3hPqLJsiNZx7yA"
            },
            {
                "https://www.youtube.com/s/player/5a3b6271/player_ias.vflset/en_US/base.js",
                "B2j7f_UPT4rfje85Lu_e",
                "m5DmNymaGQ5RdQ"
            },
            {
                "https://www.youtube.com/s/player/7a062b77/player_ias.vflset/en_US/base.js",
                "NRcE3y3mVtm_cV-W",
                "VbsCYUATvqlt5w"
            },
            {
                "https://www.youtube.com/s/player/dac945fd/player_ias.vflset/en_US/base.js",
                "o8BkRxXhuYsBCWi6RplPdP",
                "3Lx32v_hmzTm6A"
            },
            {
                "https://www.youtube.com/s/player/6f20102c/player_ias.vflset/en_US/base.js",
                "lE8DhoDmKqnmJJ",
                "pJTTX6XyJP2BYw"
            },
            {
                "https://www.youtube.com/s/player/cfa9e7cb/player_ias.vflset/en_US/base.js",
                "aCi3iElgd2kq0bxVbQ",
                "QX1y8jGb2IbZ0w"
            },
            {
                "https://www.youtube.com/s/player/8c7583ff/player_ias.vflset/en_US/base.js",
                "1wWCVpRR96eAmMI87L",
                "KSkWAVv1ZQxC3A"
            },
            {
                "https://www.youtube.com/s/player/b7910ca8/player_ias.vflset/en_US/base.js",
                "_hXMCwMt9qE310D",
                "LoZMgkkofRMCZQ"
            },
            {
                "https://www.youtube.com/s/player/590f65a6/player_ias.vflset/en_US/base.js",
                "1tm7-g_A9zsI8_Lay_",
                "xI4Vem4Put_rOg"
            },
            {
                "https://www.youtube.com/s/player/b22ef6e7/player_ias.vflset/en_US/base.js",
                "b6HcntHGkvBLk_FRf",
                "kNPW6A7FyP2l8A"
            },
            {
                "https://www.youtube.com/s/player/3400486c/player_ias.vflset/en_US/base.js",
                "lL46g3XifCKUZn1Xfw",
                "z767lhet6V2Skl"
            },
            {
                "https://www.youtube.com/s/player/20dfca59/player_ias.vflset/en_US/base.js",
                "-fLCxedkAk4LUTK2",
                "O8kfRq1y1eyHGw"
            },
            {
                "https://www.youtube.com/s/player/b12cc44b/player_ias.vflset/en_US/base.js",
                "keLa5R2U00sR9SQK",
                "N1OGyujjEwMnLw"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetPlayerTestCases))]
    public async Task I_can_decipher_N_signature(string url, string input, string expectedOutput)
    {
        var httpClient = new HttpClient();
        var playerSource = new PlayerSource(await httpClient.GetStringAsync(url));

        playerSource.NSignatureManifest.Should().NotBeNull();

        var output = playerSource.NSignatureManifest!.Decipher(input);

        output.Should().Be(expectedOutput);
    }
}
