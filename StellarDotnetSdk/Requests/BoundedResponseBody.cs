using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Reads an HTTP response body with a hard size limit, so a hostile or malfunctioning server cannot exhaust memory
///     by streaming an unbounded body. Shared by the clients that read anchor and auth-server responses themselves.
/// </summary>
internal static class BoundedResponseBody
{
    /// <summary>
    ///     Reads a response body into a string, or returns <c>null</c> when it exceeds <paramref name="maxBytes" />.
    ///     A declared <c>Content-Length</c> over the limit is refused before anything is read; an undeclared or
    ///     understated length is caught while streaming. A leading UTF-8 byte order mark is skipped, as
    ///     <see cref="HttpContent.ReadAsStringAsync()" /> would.
    /// </summary>
    /// <remarks>
    ///     The limit bounds memory only when the response was requested with
    ///     <see cref="HttpCompletionOption.ResponseHeadersRead" />; otherwise <see cref="HttpClient" /> has already
    ///     buffered the whole body.
    /// </remarks>
    /// <param name="response">The response whose body is read.</param>
    /// <param name="maxBytes">The largest body accepted, in bytes.</param>
    /// <param name="encoding">The encoding the body is decoded with.</param>
    /// <param name="cancellationToken">Cancellation token for the read.</param>
    /// <exception cref="DecoderFallbackException">
    ///     Thrown when <paramref name="encoding" /> throws on invalid input and the body is not valid in it.
    /// </exception>
    internal static async Task<string?> ReadAsStringAsync(HttpResponseMessage response, int maxBytes,
        Encoding encoding, CancellationToken cancellationToken)
    {
        // Never null on .NET 5+, but netstandard2.1 also runs on older runtimes where it can be.
        if (response.Content == null)
        {
            return string.Empty;
        }

        if (response.Content.Headers.ContentLength > maxBytes)
        {
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > maxBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        var bytes = buffer.GetBuffer();
        var length = (int)buffer.Length;
        var offset = length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        return encoding.GetString(bytes, offset, length - offset);
    }
}
