using System;
using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="RetryAfterParser" />.
/// </summary>
[TestClass]
public class RetryAfterParserTests
{
    /// <summary>
    ///     Verifies that a RetryConditionHeaderValue with a delta-seconds value is parsed correctly.
    /// </summary>
    [TestMethod]
    public void ToTimeSpan_HeaderWithDelta_ReturnsDelta()
    {
        var header = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
        Assert.AreEqual(TimeSpan.FromSeconds(30), RetryAfterParser.ToTimeSpan(header));
    }

    /// <summary>
    ///     Verifies that a RetryConditionHeaderValue with a future date is parsed to a positive TimeSpan.
    /// </summary>
    [TestMethod]
    public void ToTimeSpan_HeaderWithFutureDate_ReturnsPositiveTimeSpan()
    {
        var future = DateTimeOffset.UtcNow.AddSeconds(45);
        var header = new RetryConditionHeaderValue(future);
        var parsed = RetryAfterParser.ToTimeSpan(header);
        Assert.IsNotNull(parsed);
        Assert.IsTrue(parsed!.Value.TotalSeconds is >= 40 and <= 50,
            $"Expected ~45s, got {parsed.Value.TotalSeconds}s");
    }

    /// <summary>
    ///     Verifies that a RetryConditionHeaderValue with a past date returns null.
    /// </summary>
    [TestMethod]
    public void ToTimeSpan_HeaderWithPastDate_ReturnsNull()
    {
        var past = DateTimeOffset.UtcNow.AddSeconds(-60);
        var header = new RetryConditionHeaderValue(past);
        Assert.IsNull(RetryAfterParser.ToTimeSpan(header));
    }

    /// <summary>
    ///     Verifies that a null header returns null.
    /// </summary>
    [TestMethod]
    public void ToTimeSpan_NullHeader_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.ToTimeSpan(null));
    }

    /// <summary>
    ///     Verifies parsing string values containing seconds.
    /// </summary>
    [TestMethod]
    public void Parse_StringSeconds_ReturnsTimeSpan()
    {
        Assert.AreEqual(TimeSpan.FromSeconds(30), RetryAfterParser.Parse("30"));
    }

    /// <summary>
    ///     Verifies that a zero-seconds string is treated as null (no positive delay).
    /// </summary>
    [TestMethod]
    public void Parse_ZeroSeconds_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.Parse("0"));
    }

    /// <summary>
    ///     Verifies that an unparseable string returns null.
    /// </summary>
    [TestMethod]
    public void Parse_UnparseableString_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.Parse("not-a-number"));
    }

    /// <summary>
    ///     Verifies that an empty string returns null.
    /// </summary>
    [TestMethod]
    public void Parse_EmptyString_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.Parse(string.Empty));
    }

    /// <summary>
    ///     Verifies that a null object returns null.
    /// </summary>
    [TestMethod]
    public void Parse_Null_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.Parse(null));
    }

    /// <summary>
    ///     Verifies that an int seconds value is parsed correctly.
    /// </summary>
    [TestMethod]
    public void Parse_IntSeconds_ReturnsTimeSpan()
    {
        Assert.AreEqual(TimeSpan.FromSeconds(45), RetryAfterParser.Parse(45));
    }

    /// <summary>
    ///     Verifies that a non-positive int returns null.
    /// </summary>
    [TestMethod]
    public void Parse_NegativeInt_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.Parse(-5));
    }

    /// <summary>
    ///     Verifies that a TimeSpan value passes through directly.
    /// </summary>
    [TestMethod]
    public void Parse_TimeSpan_ReturnsTimeSpan()
    {
        Assert.AreEqual(TimeSpan.FromMinutes(2), RetryAfterParser.Parse(TimeSpan.FromMinutes(2)));
    }

    /// <summary>
    ///     Verifies that a long seconds value is parsed correctly.
    /// </summary>
    [TestMethod]
    public void Parse_LongSeconds_ReturnsTimeSpan()
    {
        Assert.AreEqual(TimeSpan.FromSeconds(60), RetryAfterParser.Parse(60L));
    }

    /// <summary>
    ///     Verifies that an HTTP-date string is parsed into the remaining delay.
    /// </summary>
    [TestMethod]
    public void Parse_HttpDateString_ReturnsPositiveTimeSpan()
    {
        var future = DateTimeOffset.UtcNow.AddSeconds(45).ToString("R", CultureInfo.InvariantCulture);
        var parsed = RetryAfterParser.Parse(future);
        Assert.IsNotNull(parsed);
        Assert.IsTrue(parsed!.Value.TotalSeconds is >= 35 and <= 50,
            $"Expected ~45s, got {parsed.Value.TotalSeconds}s");
    }

    /// <summary>
    ///     Verifies that a fractional seconds string is rejected — RFC 7231 delay-seconds is an integer, so a
    ///     fractional value must not be misread as a sub-second delay.
    /// </summary>
    [TestMethod]
    public void Parse_FractionalSecondsString_ReturnsNull()
    {
        Assert.IsNull(RetryAfterParser.Parse("2.5"));
    }

    /// <summary>
    ///     Regression for the date-fallback trap: a fractional-seconds string whose digits also form a
    ///     month.day pattern must NOT fall through to the HTTP-date parser. "12.25" used to be read as
    ///     December 25 of the current year, yielding a delay of months instead of null — and made the
    ///     "2.5" rejection above calendar-dependent (it only held while February 5 lay in the past).
    /// </summary>
    [TestMethod]
    public void Parse_FractionalSecondsString_NeverParsesAsDate()
    {
        Assert.IsNull(RetryAfterParser.Parse("12.25"));
        Assert.IsNull(RetryAfterParser.Parse("1.5"));
        Assert.IsNull(RetryAfterParser.Parse("12,25"));
    }

    /// <summary>
    ///     Pins the lenient HTTP-date acceptance: an ISO-8601 timestamp (as emitted by some proxies/CDNs)
    ///     must keep parsing after the decimal-number guard was added in front of the date fallback.
    /// </summary>
    [TestMethod]
    public void Parse_IsoDateString_StillParsesAsDate()
    {
        var future = DateTimeOffset.UtcNow.AddSeconds(45).ToString("o", CultureInfo.InvariantCulture);
        var parsed = RetryAfterParser.Parse(future);
        Assert.IsNotNull(parsed);
        Assert.IsTrue(parsed!.Value.TotalSeconds is >= 35 and <= 50,
            $"Expected ~45s, got {parsed.Value.TotalSeconds}s");
    }

    /// <summary>
    ///     Strings that are neither valid delay-seconds nor a complete RFC 1123 / ISO 8601 date must be
    ///     rejected. The lenient date fallback used to read "12.25.2030" as December 25, 2030 (a
    ///     years-long delay), "12-25"/"Dec 25" as the next December 25, and "13:45" as a time of day
    ///     whose result flipped with the wall clock.
    /// </summary>
    [TestMethod]
    public void Parse_MultiSeparatorOrPartialDateStrings_ReturnNull()
    {
        Assert.IsNull(RetryAfterParser.Parse("12.25.2030"));
        Assert.IsNull(RetryAfterParser.Parse("1.2.3"));
        Assert.IsNull(RetryAfterParser.Parse("12-25"));
        Assert.IsNull(RetryAfterParser.Parse("Dec 25"));
        Assert.IsNull(RetryAfterParser.Parse("13:45"));
    }

    /// <summary>
    ///     Delay-seconds beyond Task.Delay's representable range clamp to
    ///     <see cref="RetryAfterParser.MaxRepresentableDelay" /> instead of producing a TimeSpan that
    ///     crashes the documented <c>Task.Delay(ex.RetryAfterDelay ?? ...)</c> pattern.
    /// </summary>
    [TestMethod]
    public void Parse_HugeSecondsString_ClampsToMaxRepresentableDelay()
    {
        Assert.AreEqual(RetryAfterParser.MaxRepresentableDelay, RetryAfterParser.Parse("99999999999"));
        Assert.AreEqual(RetryAfterParser.MaxRepresentableDelay, RetryAfterParser.Parse("922337203685"));
    }

    /// <summary>
    ///     <c>TooManyRequestsException.RetryAfter</c> (int seconds) and <c>RetryAfterDelay</c> (TimeSpan)
    ///     agree only because every delay this parser yields fits in int seconds — the exception
    ///     constructors guard with <c>TotalSeconds &lt;= int.MaxValue</c>, which is currently always true.
    ///     Pin the assumption so a future increase to <see cref="RetryAfterParser.MaxRepresentableDelay" />
    ///     can't silently make that guard reachable and the two properties disagree.
    /// </summary>
    [TestMethod]
    public void MaxRepresentableDelay_FitsWithinIntSeconds()
    {
        Assert.IsTrue(RetryAfterParser.MaxRepresentableDelay.TotalSeconds <= int.MaxValue,
            "MaxRepresentableDelay must stay within int seconds, or the exception RetryAfter/RetryAfterDelay agreement breaks.");
    }

    /// <summary>
    ///     Typed DateTime/DateTimeOffset values are parsed directly — no culture-sensitive ToString round
    ///     trip. An Unspecified-kind DateTime is treated as UTC, mirroring the string path.
    /// </summary>
    /// <summary>
    ///     RFC 7231 §7.1.1.1 requires recipients to accept all three HTTP-date forms. The strict parser must
    ///     accept RFC 850 and asctime (including asctime's space-padded single-digit day) as well as RFC 1123,
    ///     so the exception path agrees with the BCL header the handler reads. The malformed-numeric guard
    ///     must still hold against these formats.
    /// </summary>
    [TestMethod]
    public void Parse_LegacyHttpDateForms_AreAccepted()
    {
        var when = DateTimeOffset.UtcNow.AddSeconds(120);
        var rfc850 = when.ToString("dddd, dd-MMM-yy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
        var asctime = when.ToString("ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture);
        // Space-padded single-digit-day asctime ("Fri Jun  5 ...") with the weekday derived from the date,
        // never hardcoded — a hardcoded weekday makes the test calendar-fragile, since TryParseExact
        // validates the weekday name against the date.
        var singleDigitDay = new DateTimeOffset(DateTime.UtcNow.Year + 2, 6, 5, 9, 10, 11, TimeSpan.Zero);
        var asctimePaddedDay = singleDigitDay.ToString("ddd MMM", CultureInfo.InvariantCulture) + "  " +
                               singleDigitDay.ToString("d HH:mm:ss yyyy", CultureInfo.InvariantCulture);

        foreach (var (label, s) in new[] { ("RFC850", rfc850), ("asctime", asctime) })
        {
            var parsed = RetryAfterParser.Parse(s);
            Assert.IsNotNull(parsed, $"{label} (\"{s}\") should parse");
            Assert.IsTrue(parsed!.Value.TotalSeconds is >= 110 and <= 125,
                $"{label}: expected ~120s, got {parsed.Value.TotalSeconds}s");
        }

        Assert.IsNotNull(RetryAfterParser.Parse(asctimePaddedDay), "space-padded single-digit asctime day must parse");

        // The malformed-numeric guard still holds — adding the legacy formats must not reopen the trap.
        Assert.IsNull(RetryAfterParser.Parse("12.25"));
        Assert.IsNull(RetryAfterParser.Parse("Dec 25"));
        Assert.IsNull(RetryAfterParser.Parse("13:45"));
    }

    [TestMethod]
    public void Parse_TypedDateValues_ReturnRemainingDelay()
    {
        AssertAboutTwoMinutes(RetryAfterParser.Parse(DateTimeOffset.UtcNow.AddSeconds(120)));
        AssertAboutTwoMinutes(RetryAfterParser.Parse(DateTime.UtcNow.AddSeconds(120)));
        AssertAboutTwoMinutes(RetryAfterParser.Parse(
            DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(120), DateTimeKind.Unspecified)));

        static void AssertAboutTwoMinutes(TimeSpan? parsed)
        {
            Assert.IsNotNull(parsed);
            Assert.IsTrue(parsed!.Value.TotalSeconds is >= 110 and <= 125,
                $"Expected ~120s, got {parsed.Value.TotalSeconds}s");
        }
    }
}