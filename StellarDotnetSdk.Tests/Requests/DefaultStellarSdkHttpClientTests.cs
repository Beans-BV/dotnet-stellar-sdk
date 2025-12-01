using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class DefaultStellarSdkHttpClientTests
{
    private static readonly Uri TestUri = new("https://example.com");

    [TestMethod]
    public async Task Constructor_WithCustomInnerHandler_UsesProvidedHandler()
    {
        // Arrange
        var trackingHandler = new TrackingHttpMessageHandler(
            (_, _, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        // Act
        using var client = new DefaultStellarSdkHttpClient(innerHandler: trackingHandler);
        await client.GetAsync(TestUri);

        // Assert
        Assert.AreEqual(1, trackingHandler.CallCount, "Custom handler should have been invoked");
    }

    [TestMethod]
    public async Task Constructor_WithCustomInnerHandlerAndResilienceOptions_ChainsCorrectly()
    {
        // Arrange
        var callCount = 0;
        var trackingHandler = new TrackingHttpMessageHandler((attempt, _, _) =>
        {
            callCount++;
            // Return 503 on first call to trigger retry, then OK
            return Task.FromResult(new HttpResponseMessage(
                attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
        });

        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false
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

    [TestMethod]
    public void Constructor_WithNullInnerHandler_UsesDefaultHandler()
    {
        // Act - should not throw, defaults to SocketsHttpHandler internally
        using var client = new DefaultStellarSdkHttpClient(innerHandler: null);

        // Assert - client was created successfully
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_SetsDefaultHeaders()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient();

        // Assert
        Assert.IsTrue(client.DefaultRequestHeaders.Contains("X-Client-Name"));
        Assert.IsTrue(client.DefaultRequestHeaders.Contains("X-Client-Version"));
    }

    [TestMethod]
    public void Constructor_WithBearerToken_SetsAuthorizationHeader()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(bearerToken: "test-token");

        // Assert
        Assert.IsNotNull(client.DefaultRequestHeaders.Authorization);
        Assert.AreEqual("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
        Assert.AreEqual("test-token", client.DefaultRequestHeaders.Authorization.Parameter);
    }

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

