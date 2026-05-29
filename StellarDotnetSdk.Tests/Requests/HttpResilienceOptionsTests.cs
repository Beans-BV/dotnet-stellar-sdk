using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="HttpResilienceOptions" /> class validation and new properties.
/// </summary>
[TestClass]
public class HttpResilienceOptionsTests
{
    /// <summary>
    ///     Verifies that HttpResilienceOptions.MaxRetryCount throws ArgumentOutOfRangeException when set to negative value.
    /// </summary>
    [TestMethod]
    public void MaxRetryCount_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxRetryCount = -1);
    }

    /// <summary>
    ///     Verifies that HttpResilienceOptions.BaseDelay throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void BaseDelay_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BaseDelay = TimeSpan.Zero);
    }

    /// <summary>
    ///     Verifies that HttpResilienceOptions.MaxDelay throws ArgumentOutOfRangeException when set to negative value.
    /// </summary>
    [TestMethod]
    public void MaxDelay_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxDelay = TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>
    ///     Verifies that HttpResilienceOptions.FailureRatio throws ArgumentOutOfRangeException when set above 1.
    /// </summary>
    [TestMethod]
    public void FailureRatio_WithValueGreaterThanOne_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.FailureRatio = 2);
    }

    /// <summary>
    ///     Verifies that HttpResilienceOptions.MinimumThroughput throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void MinimumThroughput_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MinimumThroughput = 0);
    }

    /// <summary>
    ///     Verifies that HttpResilienceOptions.SamplingDuration throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void SamplingDuration_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.SamplingDuration = TimeSpan.Zero);
    }

    /// <summary>
    ///     Verifies that HttpResilienceOptions.BreakDuration throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void BreakDuration_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BreakDuration = TimeSpan.Zero);
    }

    /// <summary>
    ///     Verifies that RetryHttpStatusCodes defaults to an empty set.
    /// </summary>
    [TestMethod]
    public void RetryHttpStatusCodes_DefaultsToEmpty()
    {
        var options = new HttpResilienceOptions();
        Assert.AreEqual(0, options.RetryHttpStatusCodes.Count);
    }

    /// <summary>
    ///     Verifies that RetryHttpStatusCodes accepts additions.
    /// </summary>
    [TestMethod]
    public void RetryHttpStatusCodes_AcceptsAdditions()
    {
        var options = new HttpResilienceOptions();
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
        Assert.AreEqual(2, options.RetryHttpStatusCodes.Count);
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.TooManyRequests));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.ServiceUnavailable));
    }

    /// <summary>
    ///     Verifies HasAnyResilienceFeatureEnabled returns true when status-code retries are configured.
    /// </summary>
    [TestMethod]
    public void HasAnyResilienceFeatureEnabled_TrueWhenStatusCodeRetriesSet()
    {
        var options = new HttpResilienceOptions { MaxRetryCount = 3 };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        Assert.IsTrue(options.HasAnyResilienceFeatureEnabled);
    }

    /// <summary>
    ///     Verifies that ForSorobanPolling preset includes the standard set of retriable status codes
    ///     and honors Retry-After while keeping its higher retry budget.
    /// </summary>
    [TestMethod]
    public void ForSorobanPolling_IncludesStatusCodeRetries()
    {
        var options = HttpResilienceOptionsPresets.ForSorobanPolling();
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.TooManyRequests));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.ServiceUnavailable));
        Assert.IsTrue(options.RespectRetryAfter);
        Assert.AreEqual(5, options.MaxRetryCount);
        Assert.AreEqual(TimeSpan.FromSeconds(15), options.MaxDelay);
    }

    /// <summary>
    ///     Verifies that the WithStandardRetries preset matches .NET industry defaults.
    /// </summary>
    [TestMethod]
    public void WithStandardRetries_MatchesIndustryDefaults()
    {
        var options = HttpResilienceOptionsPresets.WithStandardRetries();

        Assert.AreEqual(3, options.MaxRetryCount);
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), options.BaseDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(5), options.MaxDelay);
        Assert.IsTrue(options.UseJitter);
        Assert.IsTrue(options.RespectRetryAfter);
        Assert.IsFalse(options.RetryUnsafeHttpMethods);

        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.RequestTimeout));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.TooManyRequests));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.InternalServerError));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.BadGateway));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.ServiceUnavailable));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.GatewayTimeout));
    }
}