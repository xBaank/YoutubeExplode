using System;
using Jint;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Bridge.NCipher;

internal class NSignatureManifest(string code, string functionName)
{
    private readonly Engine _jsEngine =
        new(options => options.LimitMemory(4_000_000).TimeoutInterval(TimeSpan.FromSeconds(10)));

    public string Decipher(string input)
    {
        _jsEngine.Evaluate(code);
        var decryptedN = _jsEngine.Invoke(functionName, input).AsString();
        return decryptedN ?? throw new YoutubeExplodeException("Couldn't decipher n signature");
    }
}
