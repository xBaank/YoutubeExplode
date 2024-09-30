using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Jint;
using Lazy;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlayerSource(string content)
{
    private readonly Engine _jsEngine = new();

    [Lazy]
    public Func<string, string>? DecryptNSignature =>
        n =>
        {
            string pattern =
                @"(?x)
                (?:
                    \.get\(""n""\)\)&&\(b=|
                    (?:
                        b=String\.fromCharCode\(110\)|
                        (?<str_idx>[a-zA-Z0-9_$.]+)&&\(b=""nn""\[\+\1\]
                    )
                    (?:
                        ,[a-zA-Z0-9_$]+\(a\))?,c=a\.
                        (?:
                            get\(b\)|
                            [a-zA-Z0-9_$]+\[b\]\|\|null
                        )\)&&\(c=|
                    \b(?<var>[a-zA-Z0-9_$]+)=
                )(?<nfunc>[a-zA-Z0-9_$]+)(?:\[(?<idx>\d+)\])?\([a-zA-Z]\)
                (?(var),[a-zA-Z0-9_$]+\.set\(""n""\,\2\),\3\.length)";

            var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            var match = regex.Match(content);
            var name = match.Groups["nfunc"].Value;
            var idx = match.Groups["idx"].Value;

            if (match.Success)
            {
                //Extract only the function, not the whole document
                _jsEngine.SetValue("document", new { });
                _jsEngine.Evaluate(content);

                // Extract the named groups 'nfunc' and 'idx'
                var array = _jsEngine.GetValue(name).AsArray();
                var func = array[idx].AsFunctionInstance();
                var result = func.Call(n);
                Console.Write(result);
                return result.AsString();
            }

            return n;
        };

    [Lazy]
    public CipherManifest? CipherManifest
    {
        get
        {
            // Extract the signature timestamp
            var signatureTimestamp = Regex
                .Match(content, @"(?:signatureTimestamp|sts):(\d{5})")
                .Groups[1]
                .Value.NullIfWhiteSpace();

            if (string.IsNullOrWhiteSpace(signatureTimestamp))
                return null;

            // Find where the player calls the cipher functions
            var cipherCallsite = Regex
                .Match(
                    content,
                    """
                    [$_\w]+=function\([$_\w]+\){([$_\w]+)=\1\.split\(['"]{2}\);.*?return \1\.join\(['"]{2}\)}
                    """,
                    RegexOptions.Singleline
                )
                .Groups[0]
                .Value.NullIfWhiteSpace();

            if (string.IsNullOrWhiteSpace(cipherCallsite))
                return null;

            // Find the object that defines the cipher functions
            var cipherContainerName = Regex
                .Match(cipherCallsite, @"([$_\w]+)\.[$_\w]+\([$_\w]+,\d+\);")
                .Groups[1]
                .Value;

            if (string.IsNullOrWhiteSpace(cipherContainerName))
                return null;

            // Find the definition of the cipher functions
            var cipherDefinition = Regex
                .Match(
                    content,
                    $$"""
                    var {{Regex.Escape(cipherContainerName)}}={.*?};
                    """,
                    RegexOptions.Singleline
                )
                .Groups[0]
                .Value.NullIfWhiteSpace();

            if (string.IsNullOrWhiteSpace(cipherDefinition))
                return null;

            // Identify the swap cipher function
            var swapFuncName = Regex
                .Match(
                    cipherDefinition,
                    @"([$_\w]+):function\([$_\w]+,[$_\w]+\){+[^}]*?%[^}]*?}",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value.NullIfWhiteSpace();

            // Identify the splice cipher function
            var spliceFuncName = Regex
                .Match(
                    cipherDefinition,
                    @"([$_\w]+):function\([$_\w]+,[$_\w]+\){+[^}]*?splice[^}]*?}",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value.NullIfWhiteSpace();

            // Identify the reverse cipher function
            var reverseFuncName = Regex
                .Match(
                    cipherDefinition,
                    @"([$_\w]+):function\([$_\w]+\){+[^}]*?reverse[^}]*?}",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value.NullIfWhiteSpace();

            var operations = new List<ICipherOperation>();
            foreach (var statement in cipherCallsite.Split(';'))
            {
                var calledFuncName = Regex
                    .Match(statement, @"[$_\w]+\.([$_\w]+)\([$_\w]+,\d+\)")
                    .Groups[1]
                    .Value;

                if (string.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                if (string.Equals(calledFuncName, swapFuncName, StringComparison.Ordinal))
                {
                    var index = Regex
                        .Match(statement, @"\([$_\w]+,(\d+)\)")
                        .Groups[1]
                        .Value.ParseInt();

                    operations.Add(new SwapCipherOperation(index));
                }
                else if (string.Equals(calledFuncName, spliceFuncName, StringComparison.Ordinal))
                {
                    var index = Regex
                        .Match(statement, @"\([$_\w]+,(\d+)\)")
                        .Groups[1]
                        .Value.ParseInt();

                    operations.Add(new SpliceCipherOperation(index));
                }
                else if (string.Equals(calledFuncName, reverseFuncName, StringComparison.Ordinal))
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return new CipherManifest(signatureTimestamp, operations);
        }
    }
}

internal partial class PlayerSource
{
    public static PlayerSource Parse(string raw) => new(raw);
}
