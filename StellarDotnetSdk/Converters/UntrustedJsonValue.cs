using System.Text;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Renders a server-supplied JSON value for inclusion in an exception message.
/// </summary>
/// <remarks>
///     <para>
///         Every value these converters reject is untrusted in both length and content, and the message built from
///         it is routinely handed straight to a logger. Interpolating it raw makes the message as long as the
///         payload — a 2 MB <c>status</c> produced a 2,000,059-character
///         <see cref="System.Text.Json.JsonException" /> message — and lets a newline forge a log line or an ANSI
///         escape reformat an operator's terminal. For the two status converters the value is server-supplied, so
///         it is attacker-controlled whenever the caller does not operate the RPC endpoint; for
///         <see cref="EventFilterTypeJsonConverter" />, whose type is request-side only and so is never bound to a
///         server response, the value comes from whatever JSON the caller chose to deserialize.
///     </para>
///     <para>
///         The value is still worth reporting: naming the offending literal is what makes a wire-format mismatch
///         diagnosable. So it is clamped and escaped rather than dropped. Every field this is used for carries an
///         ASCII wire literal by contract — <c>PENDING</c>, <c>NOT_FOUND</c>, <c>system,contract</c> — so a
///         legitimate mismatch is well under the limit and renders exactly as it arrived.
///     </para>
/// </remarks>
internal static class UntrustedJsonValue
{
    /// <summary>
    ///     Longest run of untrusted text copied into a message, in UTF-16 code units. Comfortably longer than
    ///     every literal any of these fields can legitimately carry, so a real mismatch is never truncated.
    ///     Counting code units rather than characters means a value made entirely of astral characters is clamped
    ///     at 32 of them; that only ever reports <em>less</em> than the limit suggests, so the bound holds.
    /// </summary>
    private const int MaxEchoedLength = 64;

    /// <summary>
    ///     Worst-case rendered length: every clamped code unit escaping to six characters, plus the two quotes and
    ///     the truncation suffix. Sized so the hostile input this exists to bound never reallocates.
    /// </summary>
    private const int MaxRenderedLength = (MaxEchoedLength * 6) + 48;

    /// <summary>
    ///     Formats <paramref name="value" /> as a quoted, length-clamped, escaped fragment.
    /// </summary>
    /// <param name="value">The rejected value, as read from the JSON payload.</param>
    /// <returns>
    ///     <c>&lt;null&gt;</c> for <see langword="null" />; otherwise the value in single quotes, with every
    ///     character outside the safe set replaced by its <c>\uXXXX</c> (or <c>\UXXXXXXXX</c>) escape and, when
    ///     it exceeds <see cref="MaxEchoedLength" />, truncated with its true length in UTF-16 code units
    ///     appended.
    /// </returns>
    internal static string Describe(string? value)
    {
        if (value == null)
        {
            return "<null>";
        }

        var truncated = value.Length > MaxEchoedLength;
        var length = truncated ? MaxEchoedLength : value.Length;

        // Never cut between the two halves of a surrogate pair: the pair is escaped as one scalar below, and
        // splitting it would report a bare surrogate code unit instead of the character the server actually
        // sent, which is not a code point a reader can look up.
        if (truncated && char.IsHighSurrogate(value[length - 1]) && char.IsLowSurrogate(value[length]))
        {
            length--;
        }

        var builder = new StringBuilder(MaxRenderedLength);
        builder.Append('\'');
        for (var i = 0; i < length; i++)
        {
            var c = value[i];

            // A well-formed astral character. Escape the whole scalar rather than its two halves, so the
            // message names a code point that can be looked up. Nothing outside ASCII is ever copied through,
            // so this needs no category test: it only chooses the escape's shape.
            if (char.IsHighSurrogate(c) && i + 1 < length && char.IsLowSurrogate(value[i + 1]))
            {
                builder.Append("\\U").Append(char.ConvertToUtf32(c, value[i + 1]).ToString("x8"));
                i++;
                continue;
            }

            if (IsSafe(c))
            {
                builder.Append(c);
            }
            else
            {
                builder.Append("\\u").Append(((int)c).ToString("x4"));
            }
        }

        builder.Append('\'');
        if (truncated)
        {
            // Code units, not characters: value.Length counts UTF-16 units, so an astral character contributes
            // two. Saying "characters" would overstate the length of an astral-heavy value by up to 2x.
            builder.Append(" (truncated, ").Append(value.Length).Append(" UTF-16 code units)");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Whether <paramref name="c" /> may reach the message intact.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A whitelist, deliberately: printable ASCII only. A blacklist of "dangerous" Unicode categories
    ///         cannot be complete, and every gap is a way to forge a line or disguise the text around it —
    ///         <c>Cc</c> alone leaves <c>Zl</c>/<c>Zp</c> (U+2028/U+2029, line terminators to .NET's own
    ///         <c>ReplaceLineEndings</c> and to JavaScript) and <c>Cf</c> (U+202E and friends, which visually
    ///         reverse the rest of the line); adding those still leaves <c>Cn</c> (U+2065 and the rest of the
    ///         reserved default-ignorable range, drawn as nothing), <c>Mn</c> (combining marks that stack out of
    ///         their line), <c>Zs</c> (U+00A0 and friends, which forge alignment in fixed-width output) and
    ///         <c>Co</c>. It also leaves a version-skew hole: a code point that is <c>Cf</c> in a newer Unicode
    ///         than the running framework's tables is <c>Cn</c> today and would pass. Enumerating what is safe
    ///         has none of those failure modes.
    ///     </para>
    ///     <para>
    ///         Three ASCII characters are excluded from the safe set. The apostrophe delimits the fragment, so a
    ///         value containing one could otherwise appear to end early and pass the rest of itself off as the
    ///         message's own prose. The backslash introduces every escape this method emits, so leaving it
    ///         intact would make the encoding ambiguous: the six characters <c>\u202e</c> arriving literally on
    ///         the wire would render identically to a real U+202E, which both destroys the diagnostic value
    ///         being traded for and lets any downstream that unescapes <c>\uXXXX</c> — a JSON log renderer, a
    ///         viewer that pretty-prints escapes — re-materialise the very character this method removed.
    ///     </para>
    ///     <para>
    ///         The quotation mark is excluded for the same reason as the apostrophe, one delimiter out: it ends a
    ///         string in the formats these messages are most often written into — a JSON structured log, a CSV
    ///         export. A correct writer for either escapes it itself, so this is defence in depth rather than a
    ///         fix for one specific downstream; but the argument for escaping <c>'</c> applies to <c>"</c>
    ///         unchanged, and the cost is nil, because every literal these fields legitimately carry is
    ///         alphanumeric, an underscore, or a comma.
    ///     </para>
    /// </remarks>
    private static bool IsSafe(char c)
    {
        return c >= ' ' && c <= '~' && c != '\'' && c != '"' && c != '\\';
    }
}
