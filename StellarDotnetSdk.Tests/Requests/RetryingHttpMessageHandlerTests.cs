using System;
using System.Collections.Generic;
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
/// Unit tests for <see cref="RetryingHttpMessageHandler"/> class.
/// </summary>
[TestClass]
public class RetryingHttpMessageHandlerTests
{
    private static readonly Uri TestUri = new("https://example.com");

    /// <summary>
    /// Verifies that RetryingHttpMessageHandler does not retry when response is successful.
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
    /// Verifies that RetryingHttpMessageHandler does not retry HTTP error status codes - only connection failures are retried.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_HttpErrorStatus_DoesNotRetry()
    {
        // Arrange
        // HTTP status codes are never retried - only connection failures
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
    /// Verifies that RetryingHttpMessageHandler does not retry non-retryable HTTP status codes.
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
    /// Verifies that RetryingHttpMessageHandler retries on retryable exceptions and eventually succeeds.
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
    /// Verifies that RetryingHttpMessageHandler reuses the same request object for connection failures.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ConnectionFailure_ReusesSameRequest()
    {
        // Arrange
        // Connection failures happen before request is sent, so same request can be reused
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
        // Same request object can be reused for connection failures
        Assert.AreSame(firstAttemptRequest, secondAttemptRequest);
    }

    /// <summary>
    /// Verifies that RetryingHttpMessageHandler never retries HTTP status codes, even error codes.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_HttpStatusCodes_NeverRetried()
    {
        // Arrange
        // HTTP status codes are never retried, even if added to additional retriable codes
        // (that property no longer exists, but this test confirms behavior)
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
    /// Verifies that RetryingHttpMessageHandler retries exceptions added to AdditionalRetriableExceptionTypes.
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
    /// Verifies that RetryingHttpMessageHandler applies exponential backoff delays without jitter.
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
    /// Verifies that RetryingHttpMessageHandler respects MaxRetryCount and stops retrying after maximum attempts.
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
    /// Verifies that RetryingHttpMessageHandler throws TimeoutRejectedException when request timeout is exceeded and retries are disabled.
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
    /// Verifies that RetryingHttpMessageHandler retries TaskCanceledException from HttpClient timeout.
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
    /// Verifies that RetryingHttpMessageHandler opens circuit breaker after failure threshold is reached.
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
    /// Verifies that RetryingHttpMessageHandler does not throw BrokenCircuitException when circuit breaker is disabled.
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
    /// Verifies that RetryingHttpMessageHandler does not retry when user cancellation token is triggered.
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
    /// Verifies that RetryingHttpMessageHandler disables retries when MaxRetryCount is zero.
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
    /// Verifies that RetryingHttpMessageHandler has retries disabled by default.
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

/// <summary>
/// Unit tests for <see cref="HttpResilienceOptions"/> class validation.
/// </summary>
[TestClass]
public class HttpResilienceOptionsTests
{
    /// <summary>
    /// Verifies that HttpResilienceOptions.MaxRetryCount throws ArgumentOutOfRangeException when set to negative value.
    /// </summary>
    [TestMethod]
    public void MaxRetryCount_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxRetryCount = -1);
    }

    /// <summary>
    /// Verifies that HttpResilienceOptions.BaseDelay throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void BaseDelay_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BaseDelay = TimeSpan.Zero);
    }

    /// <summary>
    /// Verifies that HttpResilienceOptions.MaxDelay throws ArgumentOutOfRangeException when set to negative value.
    /// </summary>
    [TestMethod]
    public void MaxDelay_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxDelay = TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>
    /// Verifies that HttpResilienceOptions.FailureRatio throws ArgumentOutOfRangeException when set to value greater than 1.
    /// </summary>
    [TestMethod]
    public void FailureRatio_WithValueGreaterThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.FailureRatio = 2);
    }

    /// <summary>
    /// Verifies that HttpResilienceOptions.MinimumThroughput throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void MinimumThroughput_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MinimumThroughput = 0);
    }

    /// <summary>
    /// Verifies that HttpResilienceOptions.SamplingDuration throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void SamplingDuration_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.SamplingDuration = TimeSpan.Zero);
    }

    /// <summary>
    /// Verifies that HttpResilienceOptions.BreakDuration throws ArgumentOutOfRangeException when set to zero.
    /// </summary>
    [TestMethod]
    public void BreakDuration_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = new HttpResilienceOptions();

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BreakDuration = TimeSpan.Zero);
    }
}