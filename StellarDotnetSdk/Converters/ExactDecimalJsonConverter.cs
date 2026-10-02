using System;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Reads a decimal amount or price that the wire carries as a string (or, leniently, as a bare JSON number)
///     and rejects any value <see cref="decimal" /> cannot hold exactly, instead of rounding it.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="decimal.Parse(string, NumberStyles, IFormatProvider)" /> silently rounds a value with more
///         significant digits than the type holds — <c>"0.1234567890123456789012345678901"</c> reads as a
///         28-place approximation — and <see cref="JsonNumberHandling.AllowReadingFromString" /> inherits that. For
///         a quoted price or amount that the caller will echo back to the anchor, or multiply to check the
///         anchor's arithmetic, a silently different number is worse than an error, so this converter compares the
///         parsed scale against the digits that arrived and throws <see cref="JsonException" /> on any loss.
///         Trailing fractional zeros carry no value and are not counted, so <c>"5.00"</c> is accepted (and keeps
///         its scale: <see cref="decimal" /> preserves it).
///     </para>
///     <para>
///         The accepted grammar is a decimal literal: an optional leading <c>-</c>, one or more digits, an optional
///         <c>.</c> followed by one or more digits, and an optional exponent (<c>e</c> or <c>E</c>, an optional sign,
///         one or more digits). SEP-38 defines amounts only as strings, and a server serializing a
///         <c>java.math.BigDecimal</c> emits <c>"1E-7"</c> for a small price, so an exponent is accepted when the
///         value it denotes fits exactly; such a value is read in its shortest form, so <c>"1.50E1"</c> reads as
///         <c>15</c>, not <c>15.0</c>. A leading <c>+</c> or <c>.</c>, whitespace,
///         group separators, and culture-specific decimal separators are rejected.
///     </para>
///     <para>
///         Writes the invariant-culture string form, which round-trips exactly.
///     </para>
///     <para>
///         Attached per property with <see cref="JsonConverterAttribute" />. It is public so that a consumer's
///         source-generated <see cref="JsonSerializerContext" /> can instantiate it. Do not register it on
///         <see cref="JsonSerializerOptions.Converters" />: every <see cref="decimal" /> those options handle would
///         then be read from a quoted string as well as a bare number, rejected instead of rounded when
///         <see cref="decimal" /> cannot hold it exactly, and written as a string rather than a number.
///     </para>
/// </remarks>
public sealed class ExactDecimalJsonConverter : JsonConverter<decimal>
{
    /// <summary>The most significant digits a <see cref="decimal" /> can hold.</summary>
    private const int MaxDigits = 29;

    /// <summary>The most fractional places a <see cref="decimal" /> holds.</summary>
    private const int MaxScale = 28;

    /// <inheritdoc />
    /// <exception cref="JsonException">
    ///     Thrown when the token is neither a string nor a number, is not a decimal literal, or cannot be
    ///     represented exactly as a <see cref="decimal" />.
    /// </exception>
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string text;
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                text = reader.GetString()!;
                break;
            case JsonTokenType.Number:
                // The raw token text, not reader.GetDecimal(): the number's own digits are what must be checked
                // for loss, and they are only available before any conversion.
                text = reader.HasValueSequence
                    ? Encoding.UTF8.GetString(reader.ValueSequence.ToArray())
                    : Encoding.UTF8.GetString(reader.ValueSpan);
                break;
            default:
                throw new JsonException(
                    $"Expected a decimal value as a JSON string or number, but found a {reader.TokenType} token.");
        }

        return Parse(text);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///     Parses <paramref name="text" /> as an exact decimal, or throws.
    /// </summary>
    /// <param name="text">The literal as it arrived on the wire.</param>
    /// <returns>The exact value, with the scale the literal carried.</returns>
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="text" /> is not a decimal literal or cannot be represented exactly.
    /// </exception>
    internal static decimal Parse(string text)
    {
        if (!TryScan(text, out var literal))
        {
            throw new JsonException($"The value {UntrustedJsonValue.Describe(text)} is not a decimal literal.");
        }

        string plain;
        if (literal.HasExponent)
        {
            // decimal.Parse does not apply a long literal's exponent faithfully (it clamps it and returns a
            // different number), so an exponent form is rewritten here into the short plain literal it denotes, and
            // only that reaches the parser. A plain literal is parsed as given, which keeps its scale ("5.00" stays
            // 5.00).
            var normalized = Normalize(text, literal);
            if (normalized == null)
            {
                throw OutOfRange(text);
            }

            if (normalized.Length == 0)
            {
                throw Rounded(text);
            }

            plain = normalized;
        }
        else
        {
            plain = text;
        }

        if (!decimal.TryParse(plain, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            // TryParse fails here only on overflow: the grammar was checked above.
            throw OutOfRange(text);
        }

        // Bits 16-23 of the flags word hold the scale. A parsed value keeps every fractional digit it can, so a
        // scale smaller than the literal's last significant fractional place means the parser rounded.
        var scale = (decimal.GetBits(value)[3] >> 16) & 0xFF;
        if (scale < CountSignificantFractionDigits(plain))
        {
            throw Rounded(text);
        }

        return value;
    }

    private static JsonException Rounded(string text)
    {
        return new JsonException(
            $"The value {UntrustedJsonValue.Describe(text)} cannot be represented exactly as System.Decimal " +
            "and would be rounded.");
    }

    private static JsonException OutOfRange(string text)
    {
        return new JsonException(
            $"The value {UntrustedJsonValue.Describe(text)} is outside the range of System.Decimal.");
    }

    /// <summary>The parts of a literal that matched the grammar, as offsets into it.</summary>
    private readonly record struct Literal(bool Negative, int IntegerStart, int IntegerEnd, int FractionStart,
        int FractionEnd, bool HasExponent, long Exponent);

    /// <summary>
    ///     Validates the grammar: an optional <c>-</c>, digits, an optional <c>.</c> and digits, and an optional
    ///     exponent.
    /// </summary>
    private static bool TryScan(string text, out Literal literal)
    {
        literal = default;
        var i = 0;
        var negative = i < text.Length && text[i] == '-';
        if (negative)
        {
            i++;
        }

        var integerStart = i;
        while (i < text.Length && IsAsciiDigit(text[i]))
        {
            i++;
        }

        var integerEnd = i;
        if (integerEnd == integerStart)
        {
            return false;
        }

        var fractionStart = i;
        var fractionEnd = i;
        if (i < text.Length && text[i] == '.')
        {
            i++;
            fractionStart = i;
            while (i < text.Length && IsAsciiDigit(text[i]))
            {
                i++;
            }

            fractionEnd = i;
            if (fractionEnd == fractionStart)
            {
                return false;
            }
        }

        var hasExponent = false;
        long exponent = 0;
        if (i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            hasExponent = true;
            i++;
            var exponentNegative = false;
            if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            {
                exponentNegative = text[i] == '-';
                i++;
            }

            var exponentStart = i;
            while (i < text.Length && IsAsciiDigit(text[i]))
            {
                // Capped long before overflow; the cap stays far above any literal the 1 MiB body limit admits,
                // so a capped exponent still lands the value out of range or beyond decimal's places.
                if (exponent < 1_000_000_000)
                {
                    exponent = (exponent * 10) + (text[i] - '0');
                }

                i++;
            }

            if (i == exponentStart)
            {
                return false;
            }

            if (exponentNegative)
            {
                exponent = -exponent;
            }
        }

        if (i != text.Length)
        {
            return false;
        }

        literal = new Literal(negative, integerStart, integerEnd, fractionStart, fractionEnd, hasExponent,
            exponent);
        return true;
    }

    /// <summary>
    ///     Rewrites an exponent-form literal as the short plain literal it denotes, with insignificant zeros dropped.
    /// </summary>
    /// <returns>
    ///     The plain literal (at most 29 integer digits and 28 fractional places); <see langword="null" /> when the
    ///     value has more integer digits than <see cref="decimal" /> holds; an empty string when it has more
    ///     fractional places than decimal holds.
    /// </returns>
    private static string? Normalize(string text, Literal literal)
    {
        // The mantissa's digits, integer part then fraction, indexed as one sequence; the decimal point sits after
        // the integer digits.
        long integerCount = literal.IntegerEnd - literal.IntegerStart;
        long digitCount = integerCount + (literal.FractionEnd - literal.FractionStart);

        char DigitAt(long index)
        {
            return index < integerCount
                ? text[literal.IntegerStart + (int)index]
                : text[literal.FractionStart + (int)(index - integerCount)];
        }

        long first = 0;
        while (first < digitCount && DigitAt(first) == '0')
        {
            first++;
        }

        if (first == digitCount)
        {
            // Zero, whatever its exponent.
            return literal.Negative ? "-0" : "0";
        }

        var last = digitCount - 1;
        while (DigitAt(last) == '0')
        {
            last--;
        }

        var significant = last - first + 1;
        // Digits of the value before its decimal point (negative when it starts further right).
        var point = integerCount - first + literal.Exponent;
        if (point > MaxDigits)
        {
            return null;
        }

        // More significant digits than decimal holds are left to the scale check after parsing, which sees the
        // rounding; the literal built here stays under MaxDigits + MaxScale digits either way.
        if (significant - point > MaxScale)
        {
            return string.Empty;
        }

        var plain = new StringBuilder(MaxDigits + MaxScale + 3);
        if (literal.Negative)
        {
            plain.Append('-');
        }

        if (point <= 0)
        {
            plain.Append("0.").Append('0', (int)-point);
            for (var k = first; k <= last; k++)
            {
                plain.Append(DigitAt(k));
            }
        }
        else
        {
            for (var k = 0L; k < Math.Max(point, significant); k++)
            {
                if (k == point)
                {
                    plain.Append('.');
                }

                plain.Append(k < significant ? DigitAt(first + k) : '0');
            }
        }

        return plain.ToString();
    }

    /// <summary>
    ///     Counts the fractional digits of a plain literal up to its last non-zero one.
    /// </summary>
    private static int CountSignificantFractionDigits(string plain)
    {
        var dot = plain.IndexOf('.');
        if (dot < 0)
        {
            return 0;
        }

        var last = plain.Length - 1;
        while (last > dot && plain[last] == '0')
        {
            last--;
        }

        return last - dot;
    }

    // char.IsDigit also accepts non-ASCII decimal digits (Arabic-Indic, fullwidth, ...), which decimal.TryParse
    // with the invariant culture then rejects; checking ASCII here keeps the two in agreement.
    private static bool IsAsciiDigit(char c)
    {
        return c >= '0' && c <= '9';
    }
}
