using System;
using Jint;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Bridge.NCipher;

internal class NSignatureManifest(string code, string functionName)
{
    private readonly Engine _jsEngine = new(options => options.LimitMemory(4_000_000));

    public string Decipher(string input)
    {
        _jsEngine.Evaluate(code);
        var decryptedN = _jsEngine.Invoke(functionName, input);
        return decryptedN.AsString()
            ?? throw new YoutubeExplodeException("Couldn't decipher n signature");
    }
}
