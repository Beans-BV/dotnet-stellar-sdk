using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.CircuitBreaker;
using Polly.Timeout;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class RetryingHttpMessageHandlerTests
{
    private static readonly Uri TestUri = new("https://example.com");

    [TestMethod]
    public async Task SendAsync_SuccessfulResponse_NoRetry()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.OK)));

        using var retryHandler = new RetryingHttpMessageHandler(handler, CreateDefaultOptions());
        using var httpClient = new HttpClient(retryHandler);

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_RetryableStatus_EventuallySucceeds()
    {
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

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_NonRetryableStatus_DoesNotRetry()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.NotFound)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 3;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_RetryableException_EventuallySucceeds()
    {
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

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_AdditionalStatus_Retried()
    {
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            return Task.FromResult(CreateResponse(
                attempt == 1 ? HttpStatusCode.Conflict : HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.AdditionalRetriableStatusCodes.Add(HttpStatusCode.Conflict);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_AdditionalException_Retried()
    {
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

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_RetryAfterSecondsHeader_Honored()
    {
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            var response = CreateResponse(attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
            if (attempt == 1)
            {
                response.Headers.Add("Retry-After", "5"); // seconds
            }

            return Task.FromResult(response);
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 1;
        options.UseJitter = false;
        options.BaseDelay = TimeSpan.FromMilliseconds(10);
        options.MaxDelay = TimeSpan.FromMilliseconds(180);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        await httpClient.GetAsync(TestUri);

        Assert.AreEqual(2, handler.CallCount);
        var delay = handler.GetDelayBetweenCalls(0, 1);
        Assert.IsTrue(delay >= TimeSpan.FromMilliseconds(150), $"Expected >=150ms delay, but was {delay.TotalMilliseconds}ms");
    }

    [TestMethod]
    public async Task SendAsync_RetryAfterDateHeader_Honored()
    {
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            var response = CreateResponse(attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
            if (attempt == 1)
            {
                response.Headers.Add("Retry-After", DateTimeOffset.UtcNow.AddSeconds(1).ToString("R"));
            }

            return Task.FromResult(response);
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 1;
        options.UseJitter = false;
        options.MaxDelay = TimeSpan.FromMilliseconds(150);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        await httpClient.GetAsync(TestUri);

        Assert.AreEqual(2, handler.CallCount);
        var delay = handler.GetDelayBetweenCalls(0, 1);
        Assert.IsTrue(delay >= TimeSpan.FromMilliseconds(120),
            $"Expected >=120ms delay, but was {delay.TotalMilliseconds}ms");
    }

    [TestMethod]
    public async Task SendAsync_RetryAfterDisabled_FallsBackToExponentialBackoff()
    {
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            var response = CreateResponse(attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
            if (attempt == 1)
            {
                response.Headers.Add("Retry-After", "5");
            }

            return Task.FromResult(response);
        });

        var options = CreateDefaultOptions();
        options.HonorRetryAfterHeader = false;
        options.MaxRetryCount = 1;
        options.BaseDelay = TimeSpan.FromMilliseconds(60);
        options.MaxDelay = TimeSpan.FromMilliseconds(100);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        await httpClient.GetAsync(TestUri);

        var delay = handler.GetDelayBetweenCalls(0, 1);
        Assert.IsTrue(delay >= TimeSpan.FromMilliseconds(50) && delay <= TimeSpan.FromMilliseconds(120),
            $"Expected delay around base delay, actual: {delay.TotalMilliseconds}ms");
    }

    [TestMethod]
    public async Task SendAsync_ExponentialBackoffWithoutJitter_AddsUpDelays()
    {
        var handler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            return Task.FromResult(CreateResponse(attempt <= 2 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
        });

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.BaseDelay = TimeSpan.FromMilliseconds(40);
        options.MaxDelay = TimeSpan.FromMilliseconds(1000);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        await httpClient.GetAsync(TestUri);

        Assert.AreEqual(3, handler.CallCount);
        var totalDelay = handler.GetDelayBetweenCalls(0, 2);
        Assert.IsTrue(totalDelay >= TimeSpan.FromMilliseconds(80), $"Expected cumulative delay >=80ms, actual {totalDelay.TotalMilliseconds}ms");
    }

    [TestMethod]
    public async Task SendAsync_MaxRetryCount_IsRespected()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 2;
        options.BaseDelay = TimeSpan.FromMilliseconds(5);
        options.UseJitter = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.AreEqual(3, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_RequestTimeout_ThrowsWhenNoRetries()
    {
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

        await Assert.ThrowsExceptionAsync<TimeoutRejectedException>(() => httpClient.GetAsync(TestUri));
        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_TaskCanceledFromTimeout_Retried()
    {
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

        var response = await httpClient.GetAsync(TestUri);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_CircuitBreaker_OpensAfterThreshold()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 0;
        options.EnableCircuitBreaker = true;
        options.FailureRatio = 0.5;
        options.MinimumThroughput = 2;
        options.SamplingDuration = TimeSpan.FromSeconds(1);
        options.BreakDuration = TimeSpan.FromSeconds(1);

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // First two calls should fail but not throw, third should hit open circuit
        await httpClient.GetAsync(TestUri);
        await httpClient.GetAsync(TestUri);
        await Assert.ThrowsExceptionAsync<BrokenCircuitException>(() => httpClient.GetAsync(TestUri));
    }

    [TestMethod]
    public async Task SendAsync_CircuitBreakerDisabled_DoesNotThrow()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));

        var options = CreateDefaultOptions();
        options.MaxRetryCount = 0;
        options.EnableCircuitBreaker = false;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        // Should never throw BrokenCircuitException
        for (var i = 0; i < 5; i++)
        {
            var response = await httpClient.GetAsync(TestUri);
            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }

    [TestMethod]
    public async Task SendAsync_UserCancellation_NotRetried()
    {
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

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => httpClient.GetAsync(TestUri, cts.Token));
        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public async Task SendAsync_MaxRetryCountZero_DisablesRetries()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));
        var options = CreateDefaultOptions();
        options.MaxRetryCount = 0;

        using var retryHandler = new RetryingHttpMessageHandler(handler, options);
        using var httpClient = new HttpClient(retryHandler);

        await httpClient.GetAsync(TestUri);

        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public async Task OptionsInstancesAreIsolated()
    {
        var handler = new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(CreateResponse(HttpStatusCode.Conflict)));

        var optionsA = CreateDefaultOptions();
        optionsA.AdditionalRetriableStatusCodes.Add(HttpStatusCode.Conflict);

        var optionsB = CreateDefaultOptions();

        using var retryHandler = new RetryingHttpMessageHandler(handler, optionsB);
        using var httpClient = new HttpClient(retryHandler);

        await httpClient.GetAsync(TestUri);

        Assert.AreEqual(1, handler.CallCount);
    }

    private static HttpResilienceOptions CreateDefaultOptions()
    {
        return new HttpResilienceOptions
        {
            BaseDelay = TimeSpan.FromMilliseconds(20),
            MaxDelay = TimeSpan.FromMilliseconds(500),
            UseJitter = false,
            HonorRetryAfterHeader = true
        };
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode);
    }

    private sealed class TrackingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public TrackingHttpMessageHandler(Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            CallTimes.Add(DateTimeOffset.UtcNow);
            return _sendAsync(CallCount, request, cancellationToken);
        }
    }
}

[TestClass]
public class HttpResilienceOptionsTests
{
    [TestMethod]
    public void NegativeMaxRetryCount_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxRetryCount = -1);
    }

    [TestMethod]
    public void ZeroBaseDelay_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BaseDelay = TimeSpan.Zero);
    }

    [TestMethod]
    public void NegativeMaxDelay_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxDelay = TimeSpan.FromMilliseconds(-1));
    }

    [TestMethod]
    public void FailureRatioOutOfRange_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.FailureRatio = 2);
    }

    [TestMethod]
    public void MinimumThroughputNonPositive_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MinimumThroughput = 0);
    }

    [TestMethod]
    public void SamplingDurationNonPositive_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.SamplingDuration = TimeSpan.Zero);
    }

    [TestMethod]
    public void BreakDurationNonPositive_Throws()
    {
        var options = new HttpResilienceOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BreakDuration = TimeSpan.Zero);
    }
}

