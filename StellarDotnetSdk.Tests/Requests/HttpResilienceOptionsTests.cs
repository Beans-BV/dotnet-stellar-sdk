using System;
using System.Net;
using System.Net.Http;
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
    ///     Verifies HasAnyResilienceFeatureEnabled returns true when retries are configured alongside status codes.
    /// </summary>
    [TestMethod]
    public void HasAnyResilienceFeatureEnabled_TrueWhenStatusCodeRetriesSet()
    {
        var options = new HttpResilienceOptions { MaxRetryCount = 3 };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        Assert.IsTrue(options.HasAnyResilienceFeatureEnabled);
    }

    /// <summary>
    ///     Regression: HasAnyResilienceFeatureEnabled must NOT be true when only RetryHttpStatusCodes is
    ///     populated without a positive MaxRetryCount. Status-code retries fire from inside the retry strategy,
    ///     which BuildPipeline only constructs when MaxRetryCount > 0 — so this configuration is a no-op,
    ///     and the flag should reflect that to avoid DefaultStellarSdkHttpClient wrapping the inner handler
    ///     in a no-op resilience pipeline.
    /// </summary>
    [TestMethod]
    public void HasAnyResilienceFeatureEnabled_FalseWhenStatusCodesSetButNoRetries()
    {
        var options = new HttpResilienceOptions { MaxRetryCount = 0 };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        options.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
        Assert.IsFalse(options.HasAnyResilienceFeatureEnabled,
            "Status codes without MaxRetryCount > 0 are a no-op — flag should not report resilience enabled.");
    }

    /// <summary>
    ///     Verifies that the default RetryHttpMethods set covers only the RFC-safe methods.
    /// </summary>
    [TestMethod]
    public void RetryHttpMethods_DefaultsToSafeMethods()
    {
        var options = new HttpResilienceOptions();
        Assert.AreEqual(3, options.RetryHttpMethods.Count);
        Assert.IsTrue(options.RetryHttpMethods.Contains(HttpMethod.Get));
        Assert.IsTrue(options.RetryHttpMethods.Contains(HttpMethod.Head));
        Assert.IsTrue(options.RetryHttpMethods.Contains(HttpMethod.Options));
    }

    /// <summary>
    ///     Verifies that ForSoroban preset includes the standard transient status codes, honors Retry-After,
    ///     keeps its higher retry budget, and crucially adds POST to RetryHttpMethods (Stellar RPC routes
    ///     all JSON-RPC calls through HTTP POST — without this, the preset would silently fail to retry any
    ///     Soroban call on 429/5xx).
    /// </summary>
    [TestMethod]
    public void ForSoroban_RetriesPostOnTransientCodes()
    {
        var options = HttpResilienceOptionsPresets.ForSoroban();
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.TooManyRequests));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.ServiceUnavailable));
        Assert.IsTrue(options.RespectRetryAfter);
        Assert.IsTrue(options.RetryHttpMethods.Contains(HttpMethod.Post),
            "ForSoroban must include POST in RetryHttpMethods — every Soroban RPC call is POST.");
        Assert.AreEqual(5, options.MaxRetryCount);
        Assert.AreEqual(TimeSpan.FromSeconds(15), options.MaxDelay);
    }

    /// <summary>
    ///     Verifies that the ForHorizon preset retries the transient HTTP status codes for the methods
    ///     Horizon actually uses (GET queries + SubmitTransaction's POST) and honors Retry-After. Also
    ///     guards against silently expanding the set: PATCH/PUT/DELETE must NOT be retried, so a future
    ///     SDK method using one of those isn't auto-retried until explicitly opted in.
    /// </summary>
    [TestMethod]
    public void ForHorizon_RetriesHorizonsMethodsOnTransientCodes()
    {
        var options = HttpResilienceOptionsPresets.ForHorizon();

        Assert.AreEqual(3, options.MaxRetryCount);
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), options.BaseDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(5), options.MaxDelay);
        Assert.IsTrue(options.UseJitter);
        Assert.IsTrue(options.RespectRetryAfter);

        Assert.IsTrue(options.RetryHttpMethods.Contains(HttpMethod.Get));
        Assert.IsTrue(options.RetryHttpMethods.Contains(HttpMethod.Post),
            "ForHorizon must include POST — Server.SubmitTransaction() is POST and idempotent on Stellar.");
        Assert.IsFalse(options.RetryHttpMethods.Contains(HttpMethod.Patch),
            "ForHorizon must NOT include PATCH — the SEP-6 PATCH /transactions/{id} endpoint is not idempotent.");
        Assert.IsFalse(options.RetryHttpMethods.Contains(HttpMethod.Put));
        Assert.IsFalse(options.RetryHttpMethods.Contains(HttpMethod.Delete));

        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.RequestTimeout));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.TooManyRequests));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.InternalServerError));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.BadGateway));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.ServiceUnavailable));
        Assert.IsTrue(options.RetryHttpStatusCodes.Contains(HttpStatusCode.GatewayTimeout));
    }

    /// <summary>
    ///     Verifies that MaxRetryAfterDelay defaults to 1 minute (the dedicated Retry-After ceiling, separate
    ///     from the exponential-backoff MaxDelay).
    /// </summary>
    [TestMethod]
    public void MaxRetryAfterDelay_DefaultsToOneMinute()
    {
        var options = new HttpResilienceOptions();
        Assert.AreEqual(TimeSpan.FromMinutes(1), options.MaxRetryAfterDelay);
    }

    /// <summary>
    ///     Verifies that MaxRetryAfterDelay rejects a non-positive value.
    /// </summary>
    [TestMethod]
    public void MaxRetryAfterDelay_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxRetryAfterDelay = TimeSpan.Zero);
    }

    /// <summary>
    ///     Regression: BaseDelay and MaxDelay must be assignable in any order via an object initializer.
    ///     Setting BaseDelay (10s) before MaxDelay (30s) previously threw because BaseDelay was validated
    ///     against the default 5s MaxDelay; the relationship is now enforced at pipeline-build time instead.
    /// </summary>
    [TestMethod]
    public void BaseDelayThenMaxDelay_OrderIndependentInInitializer()
    {
        var options = new HttpResilienceOptions
        {
            BaseDelay = TimeSpan.FromSeconds(10),
            MaxDelay = TimeSpan.FromSeconds(30),
        };
        Assert.AreEqual(TimeSpan.FromSeconds(10), options.BaseDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.MaxDelay);
    }

    /// <summary>
    ///     Verifies that the deprecated ForSorobanPolling() alias forwards to ForSoroban().
    /// </summary>
    [TestMethod]
    public void ForSorobanPolling_IsObsoleteAliasForForSoroban()
    {
#pragma warning disable CS0618 // intentionally exercising the obsolete alias
        var legacy = HttpResilienceOptionsPresets.ForSorobanPolling();
#pragma warning restore CS0618
        var current = HttpResilienceOptionsPresets.ForSoroban();
        Assert.AreEqual(current.MaxRetryCount, legacy.MaxRetryCount);
        Assert.AreEqual(current.MaxDelay, legacy.MaxDelay);
        Assert.IsTrue(legacy.RetryHttpMethods.Contains(HttpMethod.Post));
        Assert.IsTrue(legacy.RetryHttpStatusCodes.Contains(HttpStatusCode.TooManyRequests));
    }

    /// <summary>
    ///     RequestTimeout outside Polly's accepted range (10ms–1 day) must fail at assignment with a clear
    ///     exception instead of surfacing Polly's ValidationException from a constructor later.
    /// </summary>
    [TestMethod]
    public void RequestTimeout_OutsidePollyBounds_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            options.RequestTimeout = TimeSpan.FromMilliseconds(5));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.RequestTimeout = TimeSpan.Zero);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            options.RequestTimeout = TimeSpan.FromSeconds(-1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.RequestTimeout = TimeSpan.FromDays(2));

        options.RequestTimeout = TimeSpan.FromSeconds(10); // valid
        options.RequestTimeout = null; // null disables the timeout — always allowed
        Assert.IsNull(options.RequestTimeout);
    }

    /// <summary>
    ///     Values below Polly's minimums (MinimumThroughput 2, Sampling/BreakDuration 500ms) or above its
    ///     1-day delay ceiling fail at assignment rather than at handler construction.
    /// </summary>
    [TestMethod]
    public void CircuitBreakerAndDelaySetters_OutsidePollyBounds_ThrowArgumentOutOfRangeException()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MinimumThroughput = 1);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            options.SamplingDuration = TimeSpan.FromMilliseconds(100));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            options.BreakDuration = TimeSpan.FromMilliseconds(100));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BaseDelay = TimeSpan.FromHours(25));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxDelay = TimeSpan.FromHours(25));
    }
}