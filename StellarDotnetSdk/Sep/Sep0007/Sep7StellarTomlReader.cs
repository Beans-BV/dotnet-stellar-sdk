using System;
using System.Globalization;
using System.Text;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>
///     Reads the top-level <c>URI_REQUEST_SIGNING_KEY</c> out of a stellar.toml in a single forward scan.
/// </summary>
/// <remarks>
///     The origin domain of a SEP-7 request is chosen by whoever wrote the URI, so its stellar.toml is untrusted
///     input. The SDK's general stellar.toml parser (Nett) recurses once per nesting level, array element,
///     dotted-key segment and table, so a few kilobytes of hostile TOML overflow the stack, and a stack overflow
///     kills the process instead of raising a catchable exception. This reader uses no recursion and no parser
///     library. It reads only the root table (everything before the first <c>[table]</c> header), which is where
///     SEP-1 puts <c>URI_REQUEST_SIGNING_KEY</c>. It checks the structure of that part (key/value lines, comments
///     and blank lines; strings decoded and terminated; arrays and inline tables balanced) but is not a full TOML
///     validator: other scalar values and the pairing of bracket kinds are not checked. Content after the first
///     table header is never looked at.
/// </remarks>
internal sealed class Sep7StellarTomlReader
{
    internal const string UriRequestSigningKeyName = "URI_REQUEST_SIGNING_KEY";

    private readonly string _toml;
    private int _position;

    private Sep7StellarTomlReader(string toml)
    {
        _toml = toml;
    }

    /// <summary>Returns the root table's <c>URI_REQUEST_SIGNING_KEY</c> string, or <c>null</c> when it has none.</summary>
    /// <exception cref="FormatException">
    ///     Thrown when the root table is not valid TOML, declares the key twice, or gives it a value that is not a
    ///     string.
    /// </exception>
    internal static string? ReadUriRequestSigningKey(string toml)
    {
        return new Sep7StellarTomlReader(toml).ReadRootTable();
    }

    private bool AtEnd => _position >= _toml.Length;

    private char Current => _toml[_position];

    private string? ReadRootTable()
    {
        if (!AtEnd && Current == '\uFEFF')
        {
            _position++;
        }

        string? signingKey = null;
        var seen = false;
        while (true)
        {
            SkipBlankLinesAndComments();
            if (AtEnd || Current == '[')
            {
                // The first [table] or [[array-of-tables]] header ends the root table.
                return signingKey;
            }

            var keyStart = _position;
            var (firstSegment, segmentCount) = ReadKey();
            SkipInlineWhitespace();
            Expect('=', "'=' after a key");
            SkipInlineWhitespace();

            if (firstSegment == UriRequestSigningKeyName)
            {
                if (segmentCount > 1)
                {
                    throw Error($"{UriRequestSigningKeyName} is used as a table", keyStart);
                }
                if (seen)
                {
                    throw Error($"{UriRequestSigningKeyName} is defined more than once", keyStart);
                }
                seen = true;
                if (AtEnd || (Current != '"' && Current != '\''))
                {
                    throw Error($"{UriRequestSigningKeyName} is not a string", _position);
                }
                signingKey = ReadString();
            }
            else
            {
                SkipValue();
            }

            SkipInlineWhitespace();
            SkipComment();
            ExpectLineEnd();
        }
    }

    private (string FirstSegment, int SegmentCount) ReadKey()
    {
        string? first = null;
        var count = 0;
        while (true)
        {
            string segment;
            if (!AtEnd && (Current == '"' || Current == '\''))
            {
                if (IsTripleQuote())
                {
                    throw Error("a multi-line string cannot be a key", _position);
                }
                segment = ReadString();
            }
            else
            {
                var start = _position;
                while (!AtEnd && IsBareKeyChar(Current))
                {
                    _position++;
                }
                if (_position == start)
                {
                    throw Error("expected a key", start);
                }
                segment = _toml.Substring(start, _position - start);
            }

            first ??= segment;
            count++;
            SkipInlineWhitespace();
            if (AtEnd || Current != '.')
            {
                return (first, count);
            }
            _position++;
            SkipInlineWhitespace();
        }
    }

    /// <summary>Skips a value: a string, an array or inline table (by balanced delimiters), or a scalar.</summary>
    private void SkipValue()
    {
        if (AtEnd)
        {
            throw Error("expected a value", _position);
        }
        switch (Current)
        {
            case '"':
            case '\'':
                ReadString();
                return;
            case '[':
            case '{':
                SkipBracketed();
                return;
            default:
                var start = _position;
                while (!AtEnd && Current != '\n' && Current != '\r' && Current != '#')
                {
                    _position++;
                }
                if (_toml.Substring(start, _position - start).Trim().Length == 0)
                {
                    throw Error("expected a value", start);
                }
                return;
        }
    }

    /// <summary>
    ///     Skips an array or inline table, including any nested ones, strings and comments inside it. Nesting is
    ///     tracked with a counter, not recursion, so its depth costs no stack.
    /// </summary>
    private void SkipBracketed()
    {
        var start = _position;
        var depth = 0;
        do
        {
            if (AtEnd)
            {
                throw Error("unterminated array or inline table", start);
            }
            switch (Current)
            {
                case '[':
                case '{':
                    depth++;
                    _position++;
                    break;
                case ']':
                case '}':
                    depth--;
                    _position++;
                    break;
                case '"':
                case '\'':
                    ReadString();
                    break;
                case '#':
                    SkipComment();
                    break;
                default:
                    _position++;
                    break;
            }
        } while (depth > 0);
    }

    /// <summary>Reads a basic, literal, multi-line basic or multi-line literal string and returns its value.</summary>
    private string ReadString()
    {
        var start = _position;
        var quote = Current;
        var multiLine = IsTripleQuote();
        _position += multiLine ? 3 : 1;
        if (multiLine)
        {
            // A newline immediately after the opening delimiter is trimmed.
            if (!AtEnd && Current == '\n')
            {
                _position++;
            }
            else if (_position + 1 < _toml.Length && Current == '\r' && _toml[_position + 1] == '\n')
            {
                _position += 2;
            }
        }

        var value = new StringBuilder();
        while (true)
        {
            if (AtEnd)
            {
                throw Error("unterminated string", start);
            }
            var c = Current;
            if (c == quote && (!multiLine || IsTripleQuote()))
            {
                if (!multiLine)
                {
                    _position++;
                    return value.ToString();
                }
                // Up to two quotes may sit directly before the closing delimiter and belong to the value.
                var run = 0;
                while (!AtEnd && Current == quote)
                {
                    run++;
                    _position++;
                }
                if (run > 5)
                {
                    throw Error("too many quotes closing a multi-line string", start);
                }
                value.Append(quote, run - 3);
                return value.ToString();
            }
            if (!multiLine && (c == '\n' || c == '\r'))
            {
                throw Error("newline in a single-line string", start);
            }
            if (quote == '"' && c == '\\')
            {
                ReadEscape(value, multiLine);
                continue;
            }
            value.Append(c);
            _position++;
        }
    }

    private void ReadEscape(StringBuilder value, bool multiLine)
    {
        var escapeStart = _position;
        _position++;
        if (AtEnd)
        {
            throw Error("unterminated escape sequence", escapeStart);
        }
        var c = Current;
        _position++;
        switch (c)
        {
            case 'b':
                value.Append('\b');
                return;
            case 't':
                value.Append('\t');
                return;
            case 'n':
                value.Append('\n');
                return;
            case 'f':
                value.Append('\f');
                return;
            case 'r':
                value.Append('\r');
                return;
            case '"':
                value.Append('"');
                return;
            case '\\':
                value.Append('\\');
                return;
            case 'u':
                value.Append(ReadUnicodeEscape(4, escapeStart));
                return;
            case 'U':
                value.Append(ReadUnicodeEscape(8, escapeStart));
                return;
        }
        if (multiLine && (c == ' ' || c == '\t' || c == '\n' || c == '\r'))
        {
            // A line-ending backslash trims the newline and the whitespace that follows it. It must be the last
            // non-whitespace character on its line: "\ " followed by more text on the same line is invalid.
            _position--;
            while (!AtEnd && (Current == ' ' || Current == '\t'))
            {
                _position++;
            }
            if (!AtEnd && Current == '\r' && _position + 1 < _toml.Length && _toml[_position + 1] == '\n')
            {
                _position++;
            }
            if (AtEnd || Current != '\n')
            {
                throw Error("invalid escape sequence", escapeStart);
            }
            while (!AtEnd && (Current == ' ' || Current == '\t' || Current == '\n' || Current == '\r'))
            {
                _position++;
            }
            return;
        }
        throw Error("invalid escape sequence", escapeStart);
    }

    private string ReadUnicodeEscape(int digits, int escapeStart)
    {
        if (_position + digits > _toml.Length ||
            !int.TryParse(_toml.Substring(_position, digits), NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture, out var codePoint) ||
            codePoint < 0 || codePoint > 0x10FFFF || (codePoint >= 0xD800 && codePoint <= 0xDFFF))
        {
            throw Error("invalid unicode escape", escapeStart);
        }
        _position += digits;
        return char.ConvertFromUtf32(codePoint);
    }

    private bool IsTripleQuote()
    {
        return _position + 2 < _toml.Length && _toml[_position + 1] == Current && _toml[_position + 2] == Current;
    }

    private void SkipBlankLinesAndComments()
    {
        while (!AtEnd)
        {
            var c = Current;
            if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
            {
                _position++;
            }
            else if (c == '#')
            {
                SkipComment();
            }
            else
            {
                return;
            }
        }
    }

    private void SkipInlineWhitespace()
    {
        while (!AtEnd && (Current == ' ' || Current == '\t'))
        {
            _position++;
        }
    }

    private void SkipComment()
    {
        if (AtEnd || Current != '#')
        {
            return;
        }
        while (!AtEnd && Current != '\n')
        {
            _position++;
        }
    }

    private void ExpectLineEnd()
    {
        if (AtEnd || Current == '\n')
        {
            return;
        }
        if (Current == '\r' && _position + 1 < _toml.Length && _toml[_position + 1] == '\n')
        {
            return;
        }
        throw Error("unexpected text after a value", _position);
    }

    private void Expect(char expected, string what)
    {
        if (AtEnd || Current != expected)
        {
            throw Error($"expected {what}", _position);
        }
        _position++;
    }

    private static bool IsBareKeyChar(char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-';
    }

    private FormatException Error(string reason, int position)
    {
        var line = 1;
        for (var i = 0; i < position && i < _toml.Length; i++)
        {
            if (_toml[i] == '\n')
            {
                line++;
            }
        }
        return new FormatException($"Invalid stellar.toml at line {line}: {reason}.");
    }
}
