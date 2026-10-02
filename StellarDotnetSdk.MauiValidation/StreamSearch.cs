using System.Text;

namespace StellarDotnetSdk.MauiValidation;

/// <summary>Searches a response stream for a piece of text while the stream arrives.</summary>
internal static class StreamSearch
{
    /// <summary>
    ///     Reads <paramref name="stream" /> until <paramref name="needle" /> appears in its UTF-8 text. Returns true as
    ///     soon as it appears, because an SSE stream does not end on its own, and false when the stream ends first.
    ///     <paramref name="onFirstBytes" />, when given, runs once, right after the first bytes arrive.
    /// </summary>
    public static async Task<bool> ReadUntilAsync(Stream stream, string needle, Action? onFirstBytes,
        CancellationToken ct)
    {
        var buffer = new byte[4096];
        var text = new StringBuilder();
        var isFirstRead = true;
        int read;
        while ((read = await stream.ReadAsync(buffer, ct)) > 0)
        {
            if (isFirstRead)
            {
                isFirstRead = false;
                onFirstBytes?.Invoke();
            }
            text.Append(Encoding.UTF8.GetString(buffer, 0, read));
            if (text.ToString().Contains(needle))
            {
                return true;
            }
        }
        return false;
    }
}
