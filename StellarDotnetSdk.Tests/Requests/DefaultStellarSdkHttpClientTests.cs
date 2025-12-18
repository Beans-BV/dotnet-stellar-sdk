using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.Timeout;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="DefaultStellarSdkHttpClient"/> class.
/// </summary>
[TestClass]
public class DefaultStellarSdkHttpClientTests
{
    private static readonly Uri TestUri = new("https://example.com");

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor uses the provided custom inner handler.
    /// </summary>
    [TestMethod]
    public async Task Constructor_WithCustomInnerHandler_UsesProvidedHandler()
    {
        // Arrange
        var trackingHandler =
            new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        // Act
        using var client = new DefaultStellarSdkHttpClient(innerHandler: trackingHandler);
        await client.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(1, trackingHandler.CallCount, "Custom handler should have been invoked");
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor correctly chains custom inner handler with resilience options.
    /// </summary>
    [TestMethod]
    public async Task Constructor_WithCustomInnerHandlerAndResilienceOptions_ChainsCorrectly()
    {
        // Arrange
        var callCount = 0;
        var trackingHandler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            callCount++;
            // Throw exception on first call to trigger retry (connection failure), then OK
            if (attempt == 1)
            {
                throw new HttpRequestException("network error");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false,
        };

        // Act
        using var client = new DefaultStellarSdkHttpClient(
            resilienceOptions: resilienceOptions,
            innerHandler: trackingHandler);
        var response = await client.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount, "Handler should be called twice due to retry");
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor uses default handler when inner handler is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullInnerHandler_UsesDefaultHandler()
    {
        // Act - should not throw, defaults to SocketsHttpHandler internally
        using var client = new DefaultStellarSdkHttpClient(innerHandler: null);

        // Assert - client was created successfully
        Assert.IsNotNull(client);
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor sets default X-Client-Name and X-Client-Version headers.
    /// </summary>
    [TestMethod]
    public void Constructor_SetsDefaultHeaders()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient();

        // Assert
        Assert.IsTrue(client.DefaultRequestHeaders.Contains("X-Client-Name"));
        Assert.IsTrue(client.DefaultRequestHeaders.Contains("X-Client-Version"));
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor sets Authorization header with Bearer token when bearer token is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBearerToken_SetsAuthorizationHeader()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient("test-token");

        // Assert
        Assert.IsNotNull(client.DefaultRequestHeaders.Authorization);
        Assert.AreEqual("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
        Assert.AreEqual("test-token", client.DefaultRequestHeaders.Authorization.Parameter);
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor sets custom X-Client-Name and X-Client-Version headers when provided.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCustomClientNameAndVersion_SetsHeaders()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(
            clientName: "my-app",
            clientVersion: "1.2.3");

        // Assert
        var clientNameHeader = client.DefaultRequestHeaders.GetValues("X-Client-Name");
        var clientVersionHeader = client.DefaultRequestHeaders.GetValues("X-Client-Version");

        Assert.IsTrue(string.Join("", clientNameHeader).Contains("my-app"));
        Assert.IsTrue(string.Join("", clientVersionHeader).Contains("1.2.3"));
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor creates resilience pipeline when only circuit breaker is enabled.
    /// </summary>
    [TestMethod]
    public async Task Constructor_WithCircuitBreakerOnly_CreatesResiliencePipeline()
    {
        // Arrange
        var trackingHandler =
            new TrackingHttpMessageHandler((_, _, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 0, // No retries
            EnableCircuitBreaker = true, // But circuit breaker enabled
        };

        // Act
        using var client = new DefaultStellarSdkHttpClient(
            resilienceOptions: resilienceOptions,
            innerHandler: trackingHandler);
        var response = await client.GetAsync(TestUri);

        // Assert - handler should be wrapped in resilience pipeline
        // Verify by checking request succeeds (pipeline is active)
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, trackingHandler.CallCount, "Handler should have been invoked through resilience pipeline");
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient constructor creates resilience pipeline when only timeout is enabled.
    /// </summary>
    [TestMethod]
    public async Task Constructor_WithTimeoutOnly_CreatesResiliencePipeline()
    {
        // Arrange
        var trackingHandler = new TrackingHttpMessageHandler((_, _, _) =>
        {
            // Return immediately - timeout should not trigger
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 0, // No retries
            RequestTimeout = TimeSpan.FromSeconds(5), // But timeout enabled
        };

        // Act
        using var client = new DefaultStellarSdkHttpClient(
            resilienceOptions: resilienceOptions,
            innerHandler: trackingHandler);
        var response = await client.GetAsync(TestUri);

        // Assert - handler should be wrapped in resilience pipeline
        // Verify by checking request succeeds (pipeline is active)
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, trackingHandler.CallCount, "Handler should have been invoked through resilience pipeline");
    }

    /// <summary>
    /// Verifies that DefaultStellarSdkHttpClient enforces request timeout when timeout is configured.
    /// </summary>
    [TestMethod]
    public async Task Constructor_WithTimeoutOnly_EnforcesTimeout()
    {
        // Arrange
        var trackingHandler = new TrackingHttpMessageHandler(async (_, _, cancellationToken) =>
        {
            // Delay longer than timeout
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 0, // No retries
            RequestTimeout = TimeSpan.FromMilliseconds(100), // Short timeout
        };

        // Act & Assert
        using var client = new DefaultStellarSdkHttpClient(
            resilienceOptions: resilienceOptions,
            innerHandler: trackingHandler);

        await Assert.ThrowsExceptionAsync<TimeoutRejectedException>(async () => { await client.GetAsync(TestUri); });
    }

    /// <summary>
    ///     A simple tracking handler for testing purposes.
    /// </summary>
    private sealed class TrackingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;
        private int _callCount;

        public TrackingHttpMessageHandler(
            Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        public int CallCount => _callCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var count = Interlocked.Increment(ref _callCount);
            return _sendAsync(count, request, cancellationToken);
        }
    }
}