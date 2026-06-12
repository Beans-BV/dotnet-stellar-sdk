using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Exceptions;

/// <summary>
///     Tests for the <c>RetryAfterDelay</c> accessor on <see cref="TooManyRequestsException" />
///     and <see cref="ServiceUnavailableException" />.
/// </summary>
[TestClass]
public class RetryAfterDelayTests
{
    /// <summary>
    ///     Verifies that TooManyRequestsException exposes RetryAfterDelay parsed from a numeric string.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_ParsesSecondsString()
    {
        var ex = new TooManyRequestsException("30");
        Assert.AreEqual(TimeSpan.FromSeconds(30), ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that TooManyRequestsException returns null RetryAfterDelay for an unparseable value.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_ReturnsNullForUnparseable()
    {
        var ex = new TooManyRequestsException("garbage");
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that TooManyRequestsException returns null RetryAfterDelay when no Retry-After is provided.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_ReturnsNullWhenAbsent()
    {
        var ex = new TooManyRequestsException();
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that ServiceUnavailableException exposes RetryAfterDelay parsed from a numeric string.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_ParsesSecondsString()
    {
        var ex = new ServiceUnavailableException("45");
        Assert.AreEqual(TimeSpan.FromSeconds(45), ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that ServiceUnavailableException returns null RetryAfterDelay for an unparseable value.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_ReturnsNullForUnparseable()
    {
        var ex = new ServiceUnavailableException("garbage");
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that ServiceUnavailableException returns null RetryAfterDelay when no Retry-After is provided.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_ReturnsNullWhenAbsent()
    {
        var ex = new ServiceUnavailableException();
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that TooManyRequestsException parses an HTTP-date Retry-After into the full remaining delay
    ///     (not just the seconds component), exercising the raw-header parsing path. Range-checked to tolerate
    ///     execution time between constructing the date and reading the delay.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_ParsesHttpDate()
    {
        var httpDate = DateTimeOffset.UtcNow.AddSeconds(90).ToString("R", CultureInfo.InvariantCulture);
        var ex = new TooManyRequestsException(httpDate);
        Assert.IsNotNull(ex.RetryAfterDelay);
        var delay = ex.RetryAfterDelay!.Value;
        Assert.IsTrue(delay > TimeSpan.FromSeconds(80) && delay <= TimeSpan.FromSeconds(90),
            $"Expected ~90s, got {delay}.");
        // RetryAfter (whole seconds) must reflect the full delay, not the 0-59 seconds component
        Assert.IsTrue(ex.RetryAfter is >= 80 and <= 91, $"Expected RetryAfter ~90, got {ex.RetryAfter}.");
    }

    /// <summary>
    ///     Verifies that ServiceUnavailableException parses an HTTP-date Retry-After into the full remaining delay.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_ParsesHttpDate()
    {
        var httpDate = DateTimeOffset.UtcNow.AddSeconds(90).ToString("R", CultureInfo.InvariantCulture);
        var ex = new ServiceUnavailableException(httpDate);
        Assert.IsNotNull(ex.RetryAfterDelay);
        var delay = ex.RetryAfterDelay!.Value;
        Assert.IsTrue(delay > TimeSpan.FromSeconds(80) && delay <= TimeSpan.FromSeconds(90),
            $"Expected ~90s, got {delay}.");
        // RetryAfter (whole seconds) must reflect the full delay, not the 0-59 seconds component
        Assert.IsTrue(ex.RetryAfter is >= 80 and <= 91, $"Expected RetryAfter ~90, got {ex.RetryAfter}.");
    }

    /// <summary>
    ///     Verifies that a Retry-After HTTP-date already in the past yields a null delay.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_ReturnsNullForPastHttpDate()
    {
        var pastDate = DateTimeOffset.UtcNow.AddSeconds(-30).ToString("R", CultureInfo.InvariantCulture);
        var ex = new TooManyRequestsException(pastDate);
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Regression: RetryAfter and RetryAfterDelay must agree for non-positive delay-seconds values.
    ///     The RetryAfterParser treats 0 and negative seconds as "no actionable delay" (null), and the
    ///     RetryAfter property must follow the same contract, otherwise the two accessors disagree.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_RetryAfterAndDelay_AgreeOnZeroString()
    {
        var ex = new TooManyRequestsException("0");
        Assert.IsNull(ex.RetryAfter, "RetryAfter for \"0\" should be null to match RetryAfterDelay.");
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Regression: same as above for a negative seconds string.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_RetryAfterAndDelay_AgreeOnNegativeString()
    {
        var ex = new TooManyRequestsException("-5");
        Assert.IsNull(ex.RetryAfter, "RetryAfter for \"-5\" should be null to match RetryAfterDelay.");
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Regression: same agreement contract for <see cref="ServiceUnavailableException" />.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_RetryAfterAndDelay_AgreeOnZeroString()
    {
        var ex = new ServiceUnavailableException("0");
        Assert.IsNull(ex.RetryAfter, "RetryAfter for \"0\" should be null to match RetryAfterDelay.");
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Regression: same as above for a negative seconds string.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_RetryAfterAndDelay_AgreeOnNegativeString()
    {
        var ex = new ServiceUnavailableException("-5");
        Assert.IsNull(ex.RetryAfter, "RetryAfter for \"-5\" should be null to match RetryAfterDelay.");
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     RetryAfterDelay must be parsed once at construction and stable across reads, like RetryAfter.
    ///     It was previously re-parsed against the current clock on every access, so two reads of the same
    ///     exception disagreed and the value eventually flipped to null.
    /// </summary>
    [TestMethod]
    public async Task TooManyRequests_RetryAfterDelay_StableAcrossReads()
    {
        var httpDate = DateTimeOffset.UtcNow.AddSeconds(90).ToString("R", CultureInfo.InvariantCulture);
        var ex = new TooManyRequestsException(httpDate);
        var first = ex.RetryAfterDelay;
        await Task.Delay(300);
        var second = ex.RetryAfterDelay;
        Assert.IsNotNull(first);
        Assert.AreEqual(first, second, "RetryAfterDelay must not change between reads of the same exception.");
    }

    /// <summary>
    ///     A fractional-seconds Retry-After ("12.25") is malformed per RFC 7231 and must not be misread as a
    ///     month.day HTTP-date (December 25), which previously produced a months-long RetryAfter/RetryAfterDelay.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_FractionalSecondsString_YieldsNull()
    {
        var ex = new TooManyRequestsException("12.25");
        Assert.IsNull(ex.RetryAfter);
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     Same fractional-seconds rejection contract for <see cref="ServiceUnavailableException" />.
    /// </summary>
    [TestMethod]
    public void ServiceUnavailable_FractionalSecondsString_YieldsNull()
    {
        var ex = new ServiceUnavailableException("12.25");
        Assert.IsNull(ex.RetryAfter);
        Assert.IsNull(ex.RetryAfterDelay);
    }

    /// <summary>
    ///     A typed DateTime passed to the constructor is parsed culture-invariantly. The old ToString
    ///     round trip under de-DE formatted a 1-hour-out instant as "11.06.2026 15:28" and invariant-parsed
    ///     it as November 6 — turning a 1-hour Retry-After into ~148 days.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_DateTimeValue_ParsesCultureInvariantly()
    {
        var savedCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var ex = new TooManyRequestsException(DateTime.UtcNow.AddHours(1));
            Assert.IsNotNull(ex.RetryAfter);
            Assert.IsTrue(ex.RetryAfter is >= 3590 and <= 3601,
                $"Expected ~3600s, got {ex.RetryAfter}s");
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }
    }

    /// <summary>
    ///     Huge delay-seconds clamp to <see cref="RetryAfterParser.MaxRepresentableDelay" />, so RetryAfter
    ///     and RetryAfterDelay stay in agreement (previously the delay was ~29,000 years while RetryAfter
    ///     was null) and the documented Task.Delay pattern cannot throw.
    /// </summary>
    [TestMethod]
    public void TooManyRequests_HugeSeconds_PropertiesAgreeOnClampedValue()
    {
        var ex = new TooManyRequestsException("922337203685");
        Assert.AreEqual(RetryAfterParser.MaxRepresentableDelay, ex.RetryAfterDelay);
        Assert.AreEqual((int)Math.Ceiling(RetryAfterParser.MaxRepresentableDelay.TotalSeconds), ex.RetryAfter);
    }
}