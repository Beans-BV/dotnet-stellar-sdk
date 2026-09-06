using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Unit tests for <see cref="UntrustedJsonValue" />, which renders a rejected server-supplied value for an
///     exception message without copying the whole payload into it.
/// </summary>
[TestClass]
public class UntrustedJsonValueTest
{
    /// <summary>
    ///     Verifies that a value of legitimate length is reproduced exactly, so a real wire-format mismatch stays
    ///     as diagnosable as it was before clamping was introduced.
    /// </summary>
    [TestMethod]
    public void Describe_WithShortValue_ReproducesItVerbatim()
    {
        Assert.AreEqual("'pending'", UntrustedJsonValue.Describe("pending"));
        Assert.AreEqual("''", UntrustedJsonValue.Describe(""));
        Assert.AreEqual("<null>", UntrustedJsonValue.Describe(null));
    }

    /// <summary>
    ///     Verifies that the message stays bounded no matter how long the server's value is. The message length
    ///     must not scale with the payload: a hostile or buggy server could otherwise turn one rejected field into
    ///     a multi-megabyte string that the caller's logger duly writes out.
    /// </summary>
    [TestMethod]
    public void Describe_WithOverlongValue_ClampsAndReportsTheTrueLength()
    {
        var described = UntrustedJsonValue.Describe(new string('A', 500_000));

        Assert.IsTrue(described.Length < 128, $"Expected a bounded fragment, got {described.Length} characters.");
        StringAssert.Contains(described, "(truncated, 500000 UTF-16 code units)");
    }

    /// <summary>
    ///     Verifies that control characters cannot reach the message intact. A raw CR/LF lets a server forge a
    ///     line in the caller's log; a raw ESC drives terminal control sequences for whoever reads it.
    /// </summary>
    [TestMethod]
    public void Describe_WithControlCharacters_EscapesThem()
    {
        var described = UntrustedJsonValue.Describe("OK\r\n2026-09-06 INFO Transaction confirmed[2J");

        Assert.IsFalse(described.Contains('\r'), "A carriage return survived into the message.");
        Assert.IsFalse(described.Contains('\n'), "A newline survived into the message.");
        Assert.IsFalse(described.Contains(''), "An ESC survived into the message.");
        StringAssert.Contains(described, "\\u000d");
        StringAssert.Contains(described, "\\u001b");
    }

    /// <summary>
    ///     End-to-end through a real converter and <c>JsonOptions.DefaultOptions</c>: the guarantee that matters is
    ///     the one an application actually observes when Stellar RPC returns a status the SDK does not accept.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOverlongStatus_ProducesABoundedMessage()
    {
        var json = "{\"hash\":\"h\",\"status\":\"" + new string('A', 500_000) + "\"}";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SendTransactionResponse>(json, JsonOptions.DefaultOptions));

        Assert.IsTrue(exception.Message.Length < 512,
            $"Exception message grew with the payload: {exception.Message.Length} characters.");
        StringAssert.Contains(exception.Message, "(truncated, 500000 UTF-16 code units)");
    }

    /// <summary>
    ///     Verifies that the clamp never cuts between the two halves of a surrogate pair. Truncating by UTF-16
    ///     code unit would leave an unpaired surrogate in the message — text that is not well-formed UTF-16, so a
    ///     strict encoder throws on it and a lenient one rewrites it to U+FFFD, corrupting the very diagnostic
    ///     this type exists to keep readable.
    /// </summary>
    [TestMethod]
    public void Describe_WhenAnAstralCharacterStraddlesTheClamp_DropsItRatherThanSplittingIt()
    {
        // The emoji is a surrogate pair whose halves sit at index 63 and 64 — exactly across the 64-char clamp.
        var straddling = new string('A', 63) + "\U0001F600" + new string('B', 1000);
        Assert.IsTrue(char.IsHighSurrogate(straddling[63]) && char.IsLowSurrogate(straddling[64]),
            "Fixture no longer straddles the clamp; the test would be vacuous.");

        // Assert the exact content, not merely that the result is well-formed: escaping a stranded surrogate
        // would ALSO be well-formed, so a well-formedness oracle alone cannot tell the clamp's surrogate
        // back-off from the escape fallback, and both blocks could be deleted with the test still passing.
        Assert.AreEqual($"'{new string('A', 63)}' (truncated, 1065 UTF-16 code units)",
            UntrustedJsonValue.Describe(straddling));

        // A pair clear of the clamp is escaped as one scalar rather than as its two surrogate halves, so the
        // message names a code point a reader can look up.
        Assert.AreEqual("'ok \\U0001f600 tail'", UntrustedJsonValue.Describe("ok \U0001F600 tail"));
        Assert.IsTrue(IsWellFormedUtf16(UntrustedJsonValue.Describe(straddling)));
    }

    /// <summary>
    ///     Verifies that astral characters are escaped as whole scalars rather than as surrogate halves. The TAG
    ///     block (U+E0020-U+E007F) spells arbitrary invisible ASCII, so a server could otherwise smuggle hidden
    ///     text into a logged message through the surrogate path.
    /// </summary>
    [TestMethod]
    public void Describe_WithAstralCharacters_EscapesThemAsWholeScalars()
    {
        // U+E004F, U+E0057 — TAG LATIN CAPITAL LETTER O / W.
        Assert.AreEqual("'BAD\\U000e004f\\U000e0057'", UntrustedJsonValue.Describe("BAD\U000E004F\U000E0057"));

        // Nothing outside printable ASCII is copied through, so a harmless astral character (emoji) is escaped
        // too. That is the whitelist working, not an over-reach: these fields carry ASCII wire literals, and no
        // category-based rule stays correct as Unicode grows.
        Assert.AreEqual("'ok\\U0001f600'", UntrustedJsonValue.Describe("ok\U0001F600"));
    }

    /// <summary>
    ///     Verifies that the escaping is <em>injective</em>: a character the method escapes and the literal text
    ///     of that escape arriving on the wire must not render identically. The backslash introduces every escape
    ///     emitted here, so leaving it intact made the encoding ambiguous — an operator could not tell which of
    ///     two payloads produced a message, and any downstream that unescapes <c>\uXXXX</c> would re-materialise
    ///     the very character the escaping removed.
    /// </summary>
    [TestMethod]
    public void Describe_WithTheTextOfAnEscape_DoesNotCollideWithTheEscapedCharacter()
    {
        var realCharacter = UntrustedJsonValue.Describe("\u202eX");
        var literalText = UntrustedJsonValue.Describe("\\u202eX");

        Assert.AreNotEqual(realCharacter, literalText,
            "A real U+202E and the six characters of its escape render identically, so the encoding is ambiguous.");
        Assert.AreEqual("'\\u202eX'", realCharacter);
        Assert.AreEqual("'\\u005cu202eX'", literalText);
    }

    /// <summary>
    ///     Verifies the categories a <c>char.IsControl</c>-style blacklist leaves behind. Each of these renders
    ///     as nothing, stacks outside its line, or forges alignment in fixed-width log output, and none is
    ///     <c>Cc</c>/<c>Cf</c>/<c>Zl</c>/<c>Zp</c>. U+2065 additionally stands in for the version-skew hole: it
    ///     is unassigned today, so no category rule written against today's Unicode tables can catch it.
    /// </summary>
    [DataTestMethod]
    [DataRow('\u2065', "\\u2065")] // reserved default-ignorable (Cn) — invisible
    [DataRow('\u00a0', "\\u00a0")] // NO-BREAK SPACE (Zs)
    [DataRow('\u0301', "\\u0301")] // COMBINING ACUTE ACCENT (Mn)
    [DataRow('\ue000', "\\ue000")] // PRIVATE USE (Co)
    [DataRow('\u007f', "\\u007f")] // DELETE (Cc, but outside the printable range)
    public void Describe_WithCharactersOutsideThePrintableAsciiSet_EscapesThem(char raw, string expectedEscape)
    {
        var described = UntrustedJsonValue.Describe("OK" + raw + "tail");

        Assert.IsFalse(described.Contains(raw), $"U+{(int)raw:X4} survived into the message unescaped.");
        StringAssert.Contains(described, expectedEscape);

        // Positive control: an ordinary printable ASCII character in the same position is NOT escaped, so the
        // rule is discriminating rather than escaping everything.
        Assert.AreEqual("'OKZtail'", UntrustedJsonValue.Describe("OKZtail"));
    }

    /// <summary>
    ///     Verifies that characters which forge a line break or reverse the display order are escaped.
    ///     U+2028/U+2029 (Zl/Zp) and U+202E/U+200B (Cf) are <em>not</em> <see cref="char.IsControl(char)" />
    ///     characters, yet .NET, JavaScript and terminal renderers all act on them, so a raw one forges a log
    ///     line or reverses it exactly as a raw CR/LF or ESC would. The final row, U+0085, <em>is</em> a control
    ///     character and serves as the positive control: it was escaped before this rule was broadened, so it
    ///     passing while the others fail identifies the category widening specifically.
    /// </summary>
    [DataTestMethod]
    [DataRow('\u2028', "\\u2028")] // LINE SEPARATOR (Zl)
    [DataRow('\u2029', "\\u2029")] // PARAGRAPH SEPARATOR (Zp)
    [DataRow('\u202e', "\\u202e")] // RIGHT-TO-LEFT OVERRIDE (Cf)
    [DataRow('\u200b', "\\u200b")] // ZERO WIDTH SPACE (Cf)
    [DataRow('\u0085', "\\u0085")] // NEXT LINE (Cc, C1)
    public void Describe_WithDisplayAlteringCharacters_EscapesThem(char raw, string expectedEscape)
    {
        var described = UntrustedJsonValue.Describe("OK" + raw + "tail");

        Assert.IsFalse(described.Contains(raw), $"U+{(int)raw:X4} survived into the message unescaped.");
        StringAssert.Contains(described, expectedEscape);
    }

    /// <summary>
    ///     Verifies that an apostrophe cannot forge the quotes delimiting the fragment, which would let a server
    ///     pass the tail of its own value off as the message's own prose.
    /// </summary>
    [TestMethod]
    public void Describe_WithApostrophe_EscapesIt()
    {
        Assert.AreEqual("'PENDING\\u0027 looks like prose'",
            UntrustedJsonValue.Describe("PENDING' looks like prose"));
    }

    /// <summary>
    ///     The quotation mark is escaped for the same reason as the apostrophe, one delimiter out: these messages
    ///     are routinely written into a JSON structured log or a CSV export, where a bare <c>"</c> ends the
    ///     string. A correct writer for either escapes it itself, so this is defence in depth — but the argument
    ///     for escaping <c>'</c> applies to <c>"</c> unchanged, and leaving only one of the pair escaped is the
    ///     asymmetry this pins.
    /// </summary>
    [TestMethod]
    public void Describe_WithQuotationMark_EscapesIt()
    {
        Assert.AreEqual("'x\\u0022,\\u0022level\\u0022:\\u0022info'",
            UntrustedJsonValue.Describe("x\",\"level\":\"info"));
    }

    /// <summary>
    ///     Pins that the truncation note counts UTF-16 code units, not characters, and says so. An astral
    ///     character costs two units, so 40 of them are 80 units and only 32 survive a 64-unit clamp — reporting
    ///     that as "80 characters" would overstate the value's length by 2x.
    /// </summary>
    [TestMethod]
    public void Describe_WithAstralCharacters_ReportsCodeUnitsNotCharacters()
    {
        var value = string.Concat(Enumerable.Repeat("\U0001F600", 40));

        var described = UntrustedJsonValue.Describe(value);

        Assert.AreEqual(80, value.Length);
        StringAssert.Contains(described, "(truncated, 80 UTF-16 code units)");
        Assert.AreEqual(32, Regex.Matches(described, @"\\U0001f600").Count);
    }

    /// <summary>
    ///     Pins the truncation boundary itself. Previously only a 500,000-character value was exercised, so the
    ///     clamp could drift by one — or flip <c>&gt;</c> to <c>&gt;=</c> — without any test noticing.
    /// </summary>
    [TestMethod]
    public void Describe_AtTheTruncationBoundary_TruncatesOnlyPastTheLimit()
    {
        Assert.AreEqual($"'{new string('A', 63)}'", UntrustedJsonValue.Describe(new string('A', 63)));
        Assert.AreEqual($"'{new string('A', 64)}'", UntrustedJsonValue.Describe(new string('A', 64)));
        Assert.AreEqual($"'{new string('A', 64)}' (truncated, 65 UTF-16 code units)",
            UntrustedJsonValue.Describe(new string('A', 65)));
    }

    /// <summary>
    ///     Verifies each strict converter actually routes its rejected value through
    ///     <see cref="UntrustedJsonValue" /> rather than interpolating it raw. Unit-testing <c>Describe</c> in
    ///     isolation proves the helper works, not that the converters call it — reverting any one of these call
    ///     sites to raw interpolation previously left the entire suite green.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(SendTransactionResponse), "{\"hash\":\"h\",\"status\":\"@\"}", 'A')]
    [DataRow(typeof(TransactionInfo), "{\"status\":\"@\"}", 'A')]
    [DataRow(typeof(SubmitTransactionAsyncResponse), "{\"hash\":\"h\",\"tx_status\":\"@\"}", 'A')]
    [DataRow(typeof(GetEventsRequest.EventFilter), "{\"type\":\"@\"}", 'A')]
    // The same rows with a payload where every character must be escaped. Escaping expands 1 char to 6, so
    // this is the real worst case; asserting only the all-'A' form would pin a bound the escaped form exceeds.
    [DataRow(typeof(SendTransactionResponse), "{\"hash\":\"h\",\"status\":\"@\"}", '\u202e')]
    [DataRow(typeof(TransactionInfo), "{\"status\":\"@\"}", '\u202e')]
    [DataRow(typeof(SubmitTransactionAsyncResponse), "{\"hash\":\"h\",\"tx_status\":\"@\"}", '\u202e')]
    [DataRow(typeof(GetEventsRequest.EventFilter), "{\"type\":\"@\"}", '\u202e')]
    public void Deserialize_WithOverlongValue_RoutesEveryConverterThroughDescribe(Type responseType,
        string jsonTemplate, char payloadChar)
    {
        var json = jsonTemplate.Replace("@", new string(payloadChar, 500_000));

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize(json, responseType, JsonOptions.DefaultOptions));

        // 64 echoed characters, each of which may expand to a 6-character \uXXXX escape, plus the longest
        // converter's surrounding prose. What matters is that the bound is a constant: it must not scale with
        // the 500,000-character payload, which is what raw interpolation would do.
        Assert.IsTrue(exception.Message.Length < 700,
            $"{responseType.Name}: the message is {exception.Message.Length} characters for a 500,000-character " +
            "payload, so this converter interpolates the raw value instead of calling UntrustedJsonValue.Describe.");
        StringAssert.Contains(exception.Message, "(truncated, 500000 UTF-16 code units)");
    }

    private static bool IsWellFormedUtf16(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsHighSurrogate(value[i]))
            {
                if (i + 1 >= value.Length || !char.IsLowSurrogate(value[i + 1]))
                {
                    return false;
                }

                i++;
            }
            else if (char.IsLowSurrogate(value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
