using System.Globalization;
using System.Text;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Renders a server-supplied JSON value for inclusion in an exception message.
/// </summary>
/// <remarks>
///     <para>
///         Every value these converters reject came off the wire, so it is attacker-controlled in both length and
///         content, and the message built from it is routinely handed straight to a logger. Interpolating it raw
///         makes the message as long as the payload — a 2 MB <c>status</c> produced a 2,000,059-character
///         <see cref="System.Text.Json.JsonException" /> message — and lets a newline forge a log line or an ANSI
///         escape reformat an operator's terminal.
///     </para>
///     <para>
///         The value is still worth reporting: naming the offending literal is what makes a wire-format mismatch
///         diagnosable. So it is clamped and escaped rather than dropped. A conforming value is well under the
///         limit and contains nothing that needs escaping, so a legitimate mismatch renders exactly as before.
///     </para>
/// </remarks>
internal static class UntrustedJsonValue
{
    /// <summary>
    ///     Longest run of server-supplied text copied into a message. Comfortably longer than every literal any
    ///     of these fields can legitimately carry, so a real mismatch is never truncated.
    /// </summary>
    private const int MaxEchoedLength = 64;

    /// <summary>
    ///     Formats <paramref name="value" /> as a quoted, length-clamped, escaped fragment.
    /// </summary>
    /// <param name="value">The rejected value, as read from the JSON payload.</param>
    /// <returns>
    ///     <c>&lt;null&gt;</c> for <see langword="null" />; otherwise the value in single quotes, with every
    ///     character that could alter how the message renders replaced by its <c>\uXXXX</c> escape and, when it
    ///     exceeds <see cref="MaxEchoedLength" />, truncated with its true length appended.
    /// </returns>
    internal static string Describe(string? value)
    {
        if (value == null)
        {
            return "<null>";
        }

        var truncated = value.Length > MaxEchoedLength;
        var length = truncated ? MaxEchoedLength : value.Length;

        // Never cut between the two halves of a surrogate pair. Clamping by UTF-16 code unit would otherwise
        // leave an unpaired surrogate in the message — text that is not well-formed UTF-16, which a strict
        // encoder rejects outright and a lenient one silently rewrites to U+FFFD, in the one place whose whole
        // job is to stay readable after it reaches a log sink.
        if (truncated && char.IsHighSurrogate(value[length - 1]) && char.IsLowSurrogate(value[length]))
        {
            length--;
        }

        var builder = new StringBuilder(MaxEchoedLength + 32);
        builder.Append('\'');
        for (var i = 0; i < length; i++)
        {
            var c = value[i];

            // A well-formed astral character. Category-test the whole scalar rather than either half — the
            // dangerous categories exist outside the BMP too, notably the TAG block U+E0020-U+E007F, an
            // invisible full-ASCII alphabet that is Cf just like U+202E. Copying the pair through unexamined
            // would leave exactly the hole NeedsEscaping closes for BMP text. This branch runs before the
            // escape test below, so that its Surrogate arm is only ever reached by an *unpaired* surrogate.
            if (char.IsHighSurrogate(c) && i + 1 < length && char.IsLowSurrogate(value[i + 1]))
            {
                if (IsEscapableCategory(CharUnicodeInfo.GetUnicodeCategory(value, i)))
                {
                    builder.Append("\\U").Append(char.ConvertToUtf32(c, value[i + 1]).ToString("x8"));
                }
                else
                {
                    builder.Append(c).Append(value[i + 1]);
                }

                i++;
                continue;
            }

            if (NeedsEscaping(c))
            {
                builder.Append("\\u").Append(((int)c).ToString("x4"));
            }
            else
            {
                builder.Append(c);
            }
        }

        builder.Append('\'');
        if (truncated)
        {
            builder.Append(" (truncated, ").Append(value.Length).Append(" characters)");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Whether <paramref name="c" /> must not reach the message intact.
    /// </summary>
    /// <remarks>
    ///     Escaping by Unicode category rather than by <see cref="char.IsControl(char)" />, which covers only
    ///     category <c>Cc</c> and so lets three separate classes of line/display forgery through:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <c>Cf</c> (Format) — U+202E RIGHT-TO-LEFT OVERRIDE and friends visually reverse the rest
    ///                 of the line in a terminal or log viewer, the Trojan-Source trick, without needing an ESC.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>Zl</c>/<c>Zp</c> — U+2028 and U+2029 are line terminators to .NET's own
    ///                 <c>ReplaceLineEndings</c> and to JavaScript, so either one forges a log line exactly the
    ///                 way a raw CR/LF would.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>Cs</c> (Surrogate) — reached only for an unpaired surrogate, since
    ///                 <see cref="Describe" /> copies valid pairs through before testing.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     The apostrophe is escaped too: the fragment is delimited by single quotes, so a value containing one
    ///     could otherwise appear to end early and pass the rest of itself off as the message's own prose.
    /// </remarks>
    private static bool NeedsEscaping(char c)
    {
        return c == '\'' || IsEscapableCategory(char.GetUnicodeCategory(c));
    }

    /// <summary>
    ///     Whether a code point in <paramref name="category" /> could forge a line or disguise the text around
    ///     it. Applied to whole scalars, so it holds for astral code points as well as BMP ones.
    /// </summary>
    private static bool IsEscapableCategory(UnicodeCategory category)
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
