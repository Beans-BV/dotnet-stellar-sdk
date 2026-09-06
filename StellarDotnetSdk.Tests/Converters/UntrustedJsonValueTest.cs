using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests.SorobanRpc;
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
        StringAssert.Contains(described, "(truncated, 500000 characters)");
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
        StringAssert.Contains(exception.Message, "(truncated, 500000 characters)");
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
        Assert.AreEqual($"'{new string('A', 63)}' (truncated, 1065 characters)",
            UntrustedJsonValue.Describe(straddling));

        // A pair clear of the clamp is copied through intact rather than escaped — pins the fast path.
        StringAssert.Contains(UntrustedJsonValue.Describe("ok \U0001F600 tail"), "\U0001F600");
        Assert.IsTrue(IsWellFormedUtf16(UntrustedJsonValue.Describe(straddling)));
    }

    /// <summary>
    ///     Verifies that the dangerous categories are escaped outside the BMP too. The TAG block
    ///     (U+E0020-U+E007F) is <c>Cf</c> exactly like U+202E and spells arbitrary invisible ASCII, so a server
    ///     could otherwise smuggle hidden text into a logged message through the surrogate path.
    /// </summary>
    [TestMethod]
    public void Describe_WithAstralFormatCharacters_EscapesThem()
    {
        // U+E004F, U+E0057 — TAG LATIN CAPITAL LETTER O / W.
        var described = UntrustedJsonValue.Describe("BAD\U000E004F\U000E0057");

        Assert.AreEqual("'BAD\\U000e004f\\U000e0057'", described);

        // Positive control: an astral character in a harmless category (emoji, So) is still copied through.
        Assert.AreEqual("'ok\U0001F600'", UntrustedJsonValue.Describe("ok\U0001F600"));
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
    ///     Pins the truncation boundary itself. Previously only a 500,000-character value was exercised, so the
    ///     clamp could drift by one — or flip <c>&gt;</c> to <c>&gt;=</c> — without any test noticing.
    /// </summary>
    [TestMethod]
    public void Describe_AtTheTruncationBoundary_TruncatesOnlyPastTheLimit()
    {
        Assert.AreEqual($"'{new string('A', 63)}'", UntrustedJsonValue.Describe(new string('A', 63)));
        Assert.AreEqual($"'{new string('A', 64)}'", UntrustedJsonValue.Describe(new string('A', 64)));
        Assert.AreEqual($"'{new string('A', 64)}' (truncated, 65 characters)",
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
    [DataRow(typeof(GetEventsRequest.EventFilter), "{\"type\":\"@\"}", 'A')]
    // The same rows with a payload where every character must be escaped. Escaping expands 1 char to 6, so
    // this is the real worst case; asserting only the all-'A' form would pin a bound the escaped form exceeds.
    [DataRow(typeof(SendTransactionResponse), "{\"hash\":\"h\",\"status\":\"@\"}", '\u202e')]
    [DataRow(typeof(TransactionInfo), "{\"status\":\"@\"}", '\u202e')]
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
        StringAssert.Contains(exception.Message, "(truncated, 500000 characters)");
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
