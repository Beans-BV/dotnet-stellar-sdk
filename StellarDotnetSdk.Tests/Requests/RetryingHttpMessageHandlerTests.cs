using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.CircuitBreaker;
using Polly.Timeout;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="RetryingHttpMessageHandler" /> class.
/// </summary>
[TestClass]
public class RetryingHttpMessageHandlerTests
{
    private static readonly Uri TestUri = new("https://example.com");

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler does not retry when response is successful.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_SuccessfulResponse_NoRetry()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.OK)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 3; // Enable retries

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that an HTTP error status code not listed in RetryHttpStatusCodes (empty by default) is returned
    ///     immediately without retrying.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_HttpErrorStatus_DoesNotRetry()
    {
        // Arrange
        // 503 is not in RetryHttpStatusCodes (empty by default), so it is returned without retry
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            if (attempt == 1)
            {
                return Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable));
            }

            return Task.FromResult(CreateResponse(HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.UseJitter = false;
        options.BaseDelay = TimeSpan.FromMilliseconds(10);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        // Should return 503 immediately without retrying
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler does not retry non-retryable HTTP status codes.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_NonRetryableStatus_DoesNotRetry()
    {
        // Arrange
        var handler =
            new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.NotFound)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 3;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler retries on retryable exceptions and eventually succeeds.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryableException_EventuallySucceeds()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            if (attempt == 1)
            {
                throw new HttpRequestException("network");
            }

            return Task.FromResult(CreateResponse(HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.BaseDelay = TimeSpan.FromMilliseconds(10);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler reuses the same request object for connection failures.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ConnectionFailure_ReusesSameRequest()
    {
        // Arrange
        // The handler reuses the same HttpRequestMessage across retries (no cloning). This is safe for connection
        // failures (the body is never read) and for the SDK's status-code retries because its request bodies are
        // buffered (StringContent / FormUrlEncodedContent) and can be re-sent.
        HttpRequestMessage? firstAttemptRequest = null;
        HttpRequestMessage? secondAttemptRequest = null;

        var handler = new TrackingHttpMessageHandler((attempt, message, _) =>
        {
            if (attempt == 1)
            {
                firstAttemptRequest = message;
                throw new HttpRequestException("network error");
            }

            secondAttemptRequest = message;
            return Task.FromResult(CreateResponse(HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 1;
        options.BaseDelay = TimeSpan.FromMilliseconds(5);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        using var originalRequest = new HttpRequestMessage(HttpMethod.Post, TestUri)
        {
            Content = new StringContent("payload"),
        };

        // Act
        var response = await httpClient.SendAsync(originalRequest);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
        Assert.IsNotNull(firstAttemptRequest);
        Assert.IsNotNull(secondAttemptRequest);
        // The same request instance reaches the inner handler on every attempt
        Assert.AreSame(firstAttemptRequest, secondAttemptRequest);
    }

    /// <summary>
    ///     Verifies that a status code not present in RetryHttpStatusCodes is returned immediately without retrying,
    ///     even when retries are enabled. (To retry specific status codes, add them to RetryHttpStatusCodes.)
    /// </summary>
    [TestMethod]
    public async Task SendAsync_StatusCodeNotInRetryList_DoesNotRetry()
    {
        // Arrange
        // 409 Conflict is not in RetryHttpStatusCodes (empty by default), so it is returned without retry
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            return Task.FromResult(CreateResponse(
                attempt == 1 ? HttpStatusCode.Conflict : HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        // Should return 409 immediately without retrying
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler retries exceptions added to AdditionalRetriableExceptionTypes.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_AdditionalException_Retried()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            if (attempt == 1)
            {
                throw new InvalidOperationException("custom");
            }

            return Task.FromResult(CreateResponse(HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.AdditionalRetriableExceptionTypes.Add(typeof(InvalidOperationException));
        options.MaxRetryCount = 1;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler applies exponential backoff delays without jitter.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ExponentialBackoffWithoutJitter_AddsUpDelays()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            if (attempt <= 2)
            {
                throw new HttpRequestException("network error");
            }

            return Task.FromResult(CreateResponse(HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.BaseDelay = TimeSpan.FromMilliseconds(40);
        options.MaxDelay = TimeSpan.FromMilliseconds(1000);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(3, handler.CallCount);
        var totalDelay = handler.GetDelayBetweenCalls(0, 2);
        Assert.IsTrue(totalDelay >= TimeSpan.FromMilliseconds(80),
            $"Expected cumulative delay >=80ms, actual {totalDelay.TotalMilliseconds}ms");
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler respects MaxRetryCount and stops retrying after maximum attempts.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_MaxRetryCount_IsRespected()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((_, _, _) => throw new HttpRequestException("network error"));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.BaseDelay = TimeSpan.FromMilliseconds(5);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => httpClient.GetAsync(TestUri));

        Assert.AreEqual(3, handler.CallCount); // Initial + 2 retries
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler throws TimeoutRejectedException when request timeout is exceeded and
    ///     retries are disabled.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RequestTimeout_ThrowsWhenNoRetries()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler(async (_, _, token) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400), token);
            return CreateResponse(HttpStatusCode.OK);
        });

        var options = CreateDefaultOptions();
        options.RequestTimeout = TimeSpan.FromMilliseconds(100);
        options.MaxRetryCount = 0;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<TimeoutRejectedException>(() => httpClient.GetAsync(TestUri));
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler retries TaskCanceledException from HttpClient timeout.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_TaskCanceledFromTimeout_Retried()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            if (attempt == 1)
            {
                throw new TaskCanceledException("HttpClient timeout");
            }

            return Task.FromResult(CreateResponse(HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 1;
        options.BaseDelay = TimeSpan.FromMilliseconds(10);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        var response = await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler opens circuit breaker after failure threshold is reached.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_CircuitBreaker_OpensAfterThreshold()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((_, _, _) => throw new HttpRequestException("network error"));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 0;
        options.EnableCircuitBreaker = true;
        options.FailureRatio = 0.5;
        options.MinimumThroughput = 2;
        options.SamplingDuration = TimeSpan.FromSeconds(1);
        options.BreakDuration = TimeSpan.FromSeconds(1);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act & Assert
        // First two calls should fail but not throw, third should hit open circuit
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => httpClient.GetAsync(TestUri));
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => httpClient.GetAsync(TestUri));
        await Assert.ThrowsExceptionAsync<BrokenCircuitException>(() => httpClient.GetAsync(TestUri));
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler does not throw BrokenCircuitException when circuit breaker is disabled.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_CircuitBreakerDisabled_DoesNotThrow()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((_, _, _) =>
            Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 0;
        options.EnableCircuitBreaker = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act & Assert
        // Should never throw BrokenCircuitException
        for (var i = 0; i < 5; i++)
        {
            var response = await httpClient.GetAsync(TestUri);
            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler does not retry when user cancellation token is triggered.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_UserCancellation_NotRetried()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler(async (_, _, token) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), token);
            return CreateResponse(HttpStatusCode.OK);
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 3;
        options.BaseDelay = TimeSpan.FromMilliseconds(10);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => httpClient.GetAsync(TestUri, cts.Token));
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler disables retries when MaxRetryCount is zero.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_MaxRetryCountZero_DisablesRetries()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((_, _, _) =>
            Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));
        var options = CreateDefaultOptions();
        options.MaxRetryCount = 0;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act
        await httpClient.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that RetryingHttpMessageHandler has retries disabled by default.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetriesDisabledByDefault()
    {
        // Arrange
        var handler = new TrackingHttpMessageHandler((_, _, _) => throw new HttpRequestException("network error"));

        // Default options have MaxRetryCount = 0
        var options = new HttpResilienceOptions();

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => httpClient.GetAsync(TestUri));

        // Should fail immediately without retrying
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that the handler retries when a configured status code is returned, then succeeds.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryHttpStatusCodes_RetriesThenSucceeds()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests),
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests),
            ScriptedHttpMessageHandler.Ok());

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false,
        };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(3, scripted.CallCount);
    }

    /// <summary>
    ///     Verifies that the handler does not retry status codes that are not in RetryHttpStatusCodes.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_StatusCodeNotInSet_DoesNotRetry()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests));

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        };
        // Deliberately NOT adding TooManyRequests.

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.AreEqual(1, scripted.CallCount);
    }

    /// <summary>
    ///     Verifies that status-code retries stop after MaxRetryCount attempts.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_StatusCodeRetries_StopAfterMaxRetryCount()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            ScriptedHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            ScriptedHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            ScriptedHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable));

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 2,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false,
        };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.AreEqual(3, scripted.CallCount); // 1 initial + 2 retries
    }

    /// <summary>
    ///     Verifies that POST requests are not retried by default, even with a configured retry status code.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_StatusCodeOnPost_NotRetriedByDefault()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests));

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.PostAsync(TestUri, new StringContent(string.Empty));

        Assert.AreEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.AreEqual(1, scripted.CallCount); // POST not retried by default
    }

    /// <summary>
    ///     Verifies that POST is retried when it is added to RetryHttpMethods.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_StatusCodeOnPost_RetriedWhenPostInRetryHttpMethods()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests),
            ScriptedHttpMessageHandler.Ok());

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);
        options.RetryHttpMethods.Add(HttpMethod.Post);

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.PostAsync(TestUri, new StringContent(string.Empty));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, scripted.CallCount);
    }

    /// <summary>
    ///     End-to-end regression test: <see cref="HttpResilienceOptionsPresets.ForSoroban" /> must
    ///     retry a POST that returns 429. Every Stellar RPC JSON-RPC call is POST, so if the preset filtered
    ///     POSTs out (the previous bug) it would silently provide no resilience for Soroban callers.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ForSorobanPreset_RetriesPostOn429()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests),
            ScriptedHttpMessageHandler.Ok());

        var options = HttpResilienceOptionsPresets.ForSoroban();
        // Use tiny delays so the test runs fast.
        options.BaseDelay = TimeSpan.FromMilliseconds(1);
        options.MaxDelay = TimeSpan.FromMilliseconds(10);
        options.UseJitter = false;

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.PostAsync(TestUri, new StringContent("{}"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, scripted.CallCount);
    }

    /// <summary>
    ///     End-to-end regression test: <see cref="HttpResilienceOptionsPresets.ForHorizon" /> must retry a
    ///     POST that returns 429. <c>Server.SubmitTransaction()</c> is POST and is idempotent on Stellar
    ///     (tx-hash + source-sequence guarantee), so if the preset filtered POSTs out it would silently
    ///     fail to retry transaction submission on transient HTTP errors.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ForHorizonPreset_RetriesPostOn429()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests),
            ScriptedHttpMessageHandler.Ok());

        var options = HttpResilienceOptionsPresets.ForHorizon();
        // Use tiny delays so the test runs fast.
        options.BaseDelay = TimeSpan.FromMilliseconds(1);
        options.MaxDelay = TimeSpan.FromMilliseconds(10);
        options.UseJitter = false;

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var response = await client.PostAsync(TestUri, new StringContent("tx=AAAA"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, scripted.CallCount);
    }

    /// <summary>
    ///     SEP-safety regression: <see cref="HttpResilienceOptionsPresets.ForHorizon" /> must NOT retry a
    ///     PATCH request, even on a configured retry status code. This guards against a developer wiring
    ///     <c>ForHorizon()</c> into a <c>TransferServerService</c> (SEP-6) HttpClient and silently
    ///     replaying <c>PATCH /transactions/{id}</c> — an endpoint that mutates KYC state and is not in
    ///     the SEP-6 master spec.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ForHorizonPreset_DoesNotRetryPatchOn429()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests));

        var options = HttpResilienceOptionsPresets.ForHorizon();
        options.BaseDelay = TimeSpan.FromMilliseconds(1);
        options.MaxDelay = TimeSpan.FromMilliseconds(10);
        options.UseJitter = false;

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Patch, TestUri)
        {
            Content = new StringContent("{}"),
        };
        var response = await client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.AreEqual(1, scripted.CallCount); // No retry — PATCH not in RetryHttpMethods
    }

    /// <summary>
    ///     Verifies that the Retry-After header value (delta-seconds form) is honored as the retry delay,
    ///     even when it exceeds the exponential-backoff <c>MaxDelay</c> — the Retry-After ceiling is the
    ///     separate <c>MaxRetryAfterDelay</c> (default 1 minute), not <c>MaxDelay</c>.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryAfterHeader_HonoredAsDelay()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests, "1"),
            ScriptedHttpMessageHandler.Ok());

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(10), // tiny backoff cap; Retry-After (1s) must still be honored
            UseJitter = false,
            RespectRetryAfter = true,
        };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync(TestUri);
        stopwatch.Stop();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, scripted.CallCount);
        Assert.IsTrue(stopwatch.Elapsed >= TimeSpan.FromMilliseconds(900),
            $"Expected at least ~1s delay from Retry-After header, got {stopwatch.Elapsed.TotalMilliseconds}ms");
    }

    /// <summary>
    ///     Verifies that the Retry-After header value is capped at MaxRetryAfterDelay (the dedicated ceiling),
    ///     not at the smaller exponential-backoff MaxDelay.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryAfterHeader_CappedAtMaxRetryAfterDelay()
    {
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests, "300"),
            ScriptedHttpMessageHandler.Ok());

        var options = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(10),
            MaxRetryAfterDelay = TimeSpan.FromMilliseconds(100),
            UseJitter = false,
            RespectRetryAfter = true,
        };
        options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);

        using var handler = new RetryingHttpMessageHandler(scripted, options);
        using var client = new HttpClient(handler);

        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync(TestUri);
        stopwatch.Stop();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // The retry waits ~100ms (Retry-After 300s capped to MaxRetryAfterDelay), proving the cap was applied
        // and the header honored — not ~0ms (header ignored) and not ~10ms (the smaller backoff MaxDelay).
        // The upper bound is deliberately generous to avoid CI flakiness; an uncapped 300s wait would hang
        // the test long before reaching this assertion.
        Assert.IsTrue(
            stopwatch.Elapsed >= TimeSpan.FromMilliseconds(80) && stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Expected Retry-After capped to ~100ms (MaxRetryAfterDelay), got {stopwatch.Elapsed.TotalMilliseconds}ms");
    }

    /// <summary>
    ///     Verifies that GET, HEAD, and OPTIONS are all retried by default for configured status codes.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_StatusCodeOnSafeMethods_RetriedByDefault()
    {
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Head, HttpMethod.Options })
        {
            var scripted = new ScriptedHttpMessageHandler(
                ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests),
                ScriptedHttpMessageHandler.Ok());

            var options = new HttpResilienceOptions
            {
                MaxRetryCount = 3,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            };
            options.RetryHttpStatusCodes.Add(HttpStatusCode.TooManyRequests);

            using var handler = new RetryingHttpMessageHandler(scripted, options);
            using var client = new HttpClient(handler);

            var response = await client.SendAsync(new HttpRequestMessage(method, TestUri));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"method={method}");
            Assert.AreEqual(2, scripted.CallCount, $"method={method}");
        }
    }

    /// <summary>
    ///     Verifies that an inconsistent BaseDelay &gt; MaxDelay (now permitted by the order-independent
    ///     property setters) is rejected when the retry pipeline is built.
    /// </summary>
    [TestMethod]
    public void Constructor_BaseDelayExceedsMaxDelay_ThrowsArgumentException()
    {
        var options = new HttpResilienceOptions { MaxRetryCount = 1 };
        options.BaseDelay = TimeSpan.FromSeconds(10); // default MaxDelay is 5s — intentionally inconsistent

        using var inner = new ScriptedHttpMessageHandler();
        Assert.ThrowsException<ArgumentException>(() => new RetryingHttpMessageHandler(inner, options));
    }

    private static HttpResilienceOptions CreateDefaultOptions()
    {
        return new HttpResilienceOptions
        {
            MaxRetryCount = 3, // Enable retries for testing
            BaseDelay = TimeSpan.FromMilliseconds(20),
            MaxDelay = TimeSpan.FromMilliseconds(500),
            UseJitter = false,
        };
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode);
    }

    private sealed class TrackingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public TrackingHttpMessageHandler(
            Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        public int CallCount { get; private set; }

        public List<DateTimeOffset> CallTimes { get; } = new();

        public TimeSpan GetDelayBetweenCalls(int firstIndex, int secondIndex)
        {
            if (CallTimes.Count <= secondIndex)
            {
                return TimeSpan.Zero;
            }

            return CallTimes[secondIndex] - CallTimes[firstIndex];
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            CallTimes.Add(DateTimeOffset.UtcNow);
            return _sendAsync(CallCount, request, cancellationToken);
        }
    }
}