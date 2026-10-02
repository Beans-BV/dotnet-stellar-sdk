using System.Globalization;
using System.Text;
using System.Text.Json;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Makes server-supplied free text, such as an anchor's <c>error</c> message or a <see cref="JsonException" />
///     message that quotes payload keys, safe to embed in an exception message, which routinely ends up in logs and
///     terminals. Unlike <see cref="UntrustedJsonValue.Describe" />, which escapes everything outside printable ASCII
///     so that a rejected wire literal is reported exactly, it keeps legitimate text in any script readable.
/// </summary>
internal static class UntrustedText
{
    /// <summary>
    ///     Longest run of server-influenced text copied into an exception message.
    /// </summary>
    internal const int MaxLength = 256;

    /// <summary>
    ///     Replaces, in any Unicode plane, control characters (<c>Cc</c>), format characters such as bidi overrides,
    ///     zero-width spaces and tag characters (<c>Cf</c>), the line and paragraph separators (<c>Zl</c>, <c>Zp</c>)
    ///     and unpaired surrogates with a space, and copies at most <paramref name="maxLength" /> UTF-16 code units
    ///     without splitting a surrogate pair, appending <c>... (truncated)</c> when it cuts the text short. This stops the text from breaking or reordering a log line. It
    ///     does not make every character visible: combining marks, variation selectors, filler letters and
    ///     private-use characters are kept, since legitimate text in many scripts needs them.
    /// </summary>
    internal static string Sanitize(string text, int maxLength = MaxLength)
    {
        var length = text.Length;
        var cut = length > maxLength ? maxLength : length;
        if (cut < length && cut > 0 && char.IsHighSurrogate(text[cut - 1]) && char.IsLowSurrogate(text[cut]))
        {
            cut--;
        }

        var builder = new StringBuilder(cut + 16);
        for (var i = 0; i < cut; i++)
        {
            var c = text[i];
            if (char.IsHighSurrogate(c) && i + 1 < cut && char.IsLowSurrogate(text[i + 1]))
            {
                // Classify the whole code point: supplementary planes hold format characters too, such as the
                // invisible Unicode tag characters (U+E0000-U+E007F) that can smuggle hidden text into a log.
                if (IsUnsafe(CharUnicodeInfo.GetUnicodeCategory(text, i)))
                {
                    builder.Append(' ');
                }
                else
                {
                    builder.Append(c).Append(text[i + 1]);
                }

                i++;
                continue;
            }

            builder.Append(IsUnsafe(CharUnicodeInfo.GetUnicodeCategory(c)) ? ' ' : c);
        }

        if (cut < length)
        {
            builder.Append("... (truncated)");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Returns a copy of a <see cref="JsonException" /> whose message is safe to log. System.Text.Json appends
    ///     the JSON path to its messages, and that path quotes dictionary keys — in a SEP-0012 body, the keys of
    ///     <c>fields</c> and <c>provided_fields</c> come from the server verbatim, with no length bound. The
    ///     original is deliberately not kept as the inner exception, because its message would carry the same
    ///     text into <see cref="System.Exception.ToString" />.
    /// </summary>
    internal static JsonException Sanitize(JsonException exception)
    {
        return new JsonException(Sanitize(exception.Message, 2 * MaxLength), null, exception.LineNumber,
            exception.BytePositionInLine);
    }

    private static bool IsUnsafe(UnicodeCategory category)
    {
        switch (category)
        {
            case UnicodeCategory.Control:
            case UnicodeCategory.Format:
            case UnicodeCategory.LineSeparator:
            case UnicodeCategory.ParagraphSeparator:
            case UnicodeCategory.Surrogate:
                return true;
            default:
                return false;
        }
    }
}
