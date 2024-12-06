using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Bridge.NCipher;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Videos.Streams;

internal class StreamController(HttpClient http) : VideoController(http)
{
    // Because we determine the player version ourselves, it's safe to cache the cipher manifest
    // for the entire lifetime of the client.
    private CipherManifest? _cipherManifest;
    private NSignatureManifest? _NcipherManifest;
    private PlayerSource? _playerSource;

    public async ValueTask<PlayerSource> GetPlayerSourceAsync(
        CancellationToken cancellationToken = default
    )
    {
        var iframe = await Http.GetStringAsync(
            "https://www.youtube.com/iframe_api",
            cancellationToken
        );

        var version = Regex.Match(iframe, @"player\\?/([0-9a-fA-F]{8})\\?/").Groups[1].Value;
        if (string.IsNullOrWhiteSpace(version))
            throw new YoutubeExplodeException("Failed to extract the player version.");

        return PlayerSource.Parse(
            await Http.GetStringAsync(
                $"https://www.youtube.com/s/player/{version}/player_ias.vflset/en_US/base.js",
                cancellationToken
            )
        );
    }

    public async ValueTask<DashManifest> GetDashManifestAsync(
        string url,
        CancellationToken cancellationToken = default
    ) => DashManifest.Parse(await Http.GetStringAsync(url, cancellationToken));

    public async ValueTask<CipherManifest> ResolveCipherManifestAsync(
        CancellationToken cancellationToken
    )
    {
        if (_cipherManifest is not null)
            return _cipherManifest;

        _playerSource ??= await GetPlayerSourceAsync(cancellationToken);

        return _cipherManifest =
            _playerSource.CipherManifest
            ?? throw new YoutubeExplodeException("Failed to extract the cipher manifest.");
    }

    public async ValueTask<NSignatureManifest> ResolveNSignatureAsync(
        CancellationToken cancellationToken
    )
    {
        if (_NcipherManifest is not null)
            return _NcipherManifest;

        _playerSource ??= await GetPlayerSourceAsync(cancellationToken);

        return _NcipherManifest =
            _playerSource.NSignatureManifest
            ?? throw new YoutubeExplodeException("Failed to extract the n signature.");
    }
}
