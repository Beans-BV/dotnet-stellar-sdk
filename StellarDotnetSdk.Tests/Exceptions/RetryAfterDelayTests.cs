using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;

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
}