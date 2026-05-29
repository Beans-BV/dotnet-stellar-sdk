using System;
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
}