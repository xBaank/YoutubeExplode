using System.Text.RegularExpressions;

namespace YoutubeExplode.Utils;

internal static class Js
{
    public static string? ExtractFunction(string jscode, string funcname)
    {
        // Regex to find the function declaration up to the first opening brace
        string pattern = $@"{Regex.Escape(funcname)}\s*=\s*function\s*\(.*?\)\s*\{{";
        Match match = Regex.Match(jscode, pattern);

        if (!match.Success)
        {
            return null; // No function found
        }

        int startIdx = match.Index + match.Length - 1; // Start just before the opening brace '{'

        // Now let's find the matching closing brace, considering nested braces
        int braceCount = 0;
        for (int i = startIdx; i < jscode.Length; i++)
        {
            if (jscode[i] == '{')
            {
                braceCount++;
            }
            else if (jscode[i] == '}')
            {
                braceCount--;
            }

            if (braceCount == 0)
            {
                // When all braces are matched, return the full function code
                return jscode.Substring(match.Index, i - match.Index + 1);
            }
        }

        return null; // If no matching closing brace is found
    }
}
