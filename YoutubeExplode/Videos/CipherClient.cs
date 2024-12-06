using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Bridge.NCipher;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Videos;

internal class CipherClient(StreamController _controller)
{
    private CipherManifest? _cipherManifest;
    private NSignatureManifest? _NcipherManifest;
    private PlayerSource? _playerSource;

    protected async ValueTask<CipherManifest> ResolveCipherManifestAsync(
        CancellationToken cancellationToken
    )
    {
        if (_cipherManifest is not null)
            return _cipherManifest;

        _playerSource ??= await _controller.GetPlayerSourceAsync(cancellationToken);

        return _cipherManifest =
            _playerSource.CipherManifest
            ?? throw new YoutubeExplodeException("Failed to extract the cipher manifest.");
    }

    protected async ValueTask<NSignatureManifest> ResolveNSignatureAsync(
        CancellationToken cancellationToken
    )
    {
        if (_NcipherManifest is not null)
            return _NcipherManifest;

        _playerSource ??= await _controller.GetPlayerSourceAsync(cancellationToken);

        return _NcipherManifest =
            _playerSource.NSignatureManifest
            ?? throw new YoutubeExplodeException("Failed to extract the n signature.");
    }
}
