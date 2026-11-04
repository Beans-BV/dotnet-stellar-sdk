using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Requests;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for <see cref="Page{T}" /> navigation (<see cref="Page{T}.NextPage" /> and
///     <see cref="Page{T}.PreviousPage" />).
/// </summary>
[TestClass]
public class PageTest
{
    // The .invalid TLD never resolves, so a follow that escapes the configured client fails fast instead of
    // reaching a real Horizon.
    private const string HorizonUri = "https://horizon.example.invalid";
    private const string SelfHref = HorizonUri + "/transactions?cursor=&limit=2&order=asc";
    private const string NextHref = HorizonUri + "/transactions?cursor=200&limit=2&order=asc";
    private const string PrevHref = HorizonUri + "/transactions?cursor=100&limit=2&order=desc";

    private const string PageJson = $$"""
                                      {
                                        "_links": {
                                          "self": { "href": "{{SelfHref}}" },
                                          "next": { "href": "{{NextHref}}" },
                                          "prev": { "href": "{{PrevHref}}" }
                                        },
                                        "_embedded": { "records": [] }
                                      }
                                      """;

    /// <summary>
    ///     Verifies that NextPage and PreviousPage on a page fetched through a <see cref="Server" /> send their
    ///     requests through the server's <see cref="HttpClient" />, so the bearer token and the X-Client headers
    ///     reach every page and not only the first.
    /// </summary>
    [TestMethod]
    public async Task NextPageAndPreviousPage_OnPageFromServer_ReuseServerHttpClient()
    {
        // Arrange
        var handler = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Ok(PageJson));
        using var httpClient = new DefaultStellarSdkHttpClient("my-token", innerHandler: handler);
        using var server = new Server(HorizonUri, httpClient);

        // Act
        var firstPage = await server.Transactions.Execute();
        var nextPage = await firstPage.NextPage()!;
        await nextPage.PreviousPage()!;

        // Assert
        Assert.AreEqual(3, handler.CallCount);
        Assert.AreEqual(NextHref, handler.Requests[1].RequestUri!.ToString());
        Assert.AreEqual(PrevHref, handler.Requests[2].RequestUri!.ToString());
        foreach (var request in handler.Requests)
        {
            Assert.AreEqual("Bearer my-token", request.Headers.Authorization?.ToString());
            Assert.AreEqual("stellar-dotnet-sdk", request.Headers.GetValues("X-Client-Name").Single());
        }
    }

    /// <summary>
    ///     Verifies that NextPage and PreviousPage send their request to the scheme, host and port the page was
    ///     fetched from, whatever the response's link names, so a response cannot route the bearer token to another
    ///     host, to another port or over plain HTTP. The path and query of the link are kept as they are.
    /// </summary>
    [TestMethod]
    [DataRow("https://evil.example.invalid/steal?cursor=1", HorizonUri + "/steal?cursor=1",
        DisplayName = "Other host")]
    [DataRow("http://horizon.example.invalid/transactions?cursor=1", HorizonUri + "/transactions?cursor=1",
        DisplayName = "Plain HTTP")]
    [DataRow("https://horizon.example.invalid:8443/transactions?cursor=1", HorizonUri + "/transactions?cursor=1",
        DisplayName = "Other port")]
    [DataRow("https://user:pass@evil.example.invalid/transactions?cursor=a%2Fb&order=asc",
        HorizonUri + "/transactions?cursor=a%2Fb&order=asc", DisplayName = "User info and escaped query")]
    public async Task NextPageAndPreviousPage_LinkToOtherOrigin_StayOnOriginOfFetchedPage(string href,
        string expectedUri)
    {
        // Arrange
        var json = PageJsonWithLinks(href, href);
        var handler = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(json),
            ScriptedHttpMessageHandler.Ok(json),
            ScriptedHttpMessageHandler.Ok(json));
        using var httpClient = new DefaultStellarSdkHttpClient("my-token", innerHandler: handler);
        using var server = new Server(HorizonUri, httpClient);

        // Act
        var firstPage = await server.Transactions.Execute();
        var nextPage = await firstPage.NextPage()!;
        await nextPage.PreviousPage()!;

        // Assert
        Assert.AreEqual(3, handler.CallCount);
        Assert.AreEqual(expectedUri, handler.Requests[1].RequestUri!.AbsoluteUri);
        Assert.AreEqual(expectedUri, handler.Requests[2].RequestUri!.AbsoluteUri);
        foreach (var request in handler.Requests)
        {
            Assert.AreEqual("Bearer my-token", request.Headers.Authorization?.ToString());
        }
    }

    /// <summary>
    ///     Verifies that a page fetched from a server on a non-default port keeps that port (and scheme) when its
    ///     link names another origin, as with a local Horizon on <c>http://localhost:8000</c>.
    /// </summary>
    [TestMethod]
    public async Task NextPage_ServerOnNonDefaultPort_StaysOnThatPort()
    {
        // Arrange
        const string serverUri = "http://horizon.example.invalid:8000";
        var json = PageJsonWithLinks("https://evil.example.invalid/transactions?cursor=1", PrevHref);
        var handler = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(json),
            ScriptedHttpMessageHandler.Ok(json));
        using var httpClient = new DefaultStellarSdkHttpClient("my-token", innerHandler: handler);
        using var server = new Server(serverUri, httpClient);

        // Act
        var firstPage = await server.Transactions.Execute();
        await firstPage.NextPage()!;

        // Assert
        Assert.AreEqual(serverUri + "/transactions?cursor=1", handler.Requests[1].RequestUri!.AbsoluteUri);
    }

    /// <summary>
    ///     Verifies that each page keeps the client that fetched it: with two servers alive, paging a page from the
    ///     first server goes through the first server's client, not the client that was used last.
    /// </summary>
    [TestMethod]
    public async Task NextPage_WithTwoServers_FollowsWithTheClientThatFetchedThePage()
    {
        // Arrange
        var handlerA = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Ok(PageJson));
        var handlerB = new ScriptedHttpMessageHandler(ScriptedHttpMessageHandler.Ok(PageJson));
        using var clientA = new DefaultStellarSdkHttpClient("token-a", innerHandler: handlerA);
        using var clientB = new DefaultStellarSdkHttpClient("token-b", innerHandler: handlerB);
        using var serverA = new Server(HorizonUri, clientA);
        using var serverB = new Server(HorizonUri, clientB);

        // Act
        var pageA = await serverA.Transactions.Execute();
        await serverB.Transactions.Execute();
        await pageA.NextPage()!;

        // Assert
        Assert.AreEqual(2, handlerA.CallCount);
        Assert.AreEqual(1, handlerB.CallCount);
        Assert.AreEqual("Bearer token-a", handlerA.Requests[1].Headers.Authorization?.ToString());
    }

    /// <summary>
    ///     Verifies that a page records the client that sent its request even when another server creates a request
    ///     builder while that request is in flight (which replaces the client the builders share), so its NextPage
    ///     does not switch to the other server's client.
    /// </summary>
    [TestMethod]
    public async Task NextPage_WhenAnotherServerCreatesABuilderDuringTheRequest_FollowsWithTheSendingClient()
    {
        // Arrange
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handlerA = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Ok(PageJson));
        var handlerB = new ScriptedHttpMessageHandler();
        using var clientA = new DefaultStellarSdkHttpClient("token-a",
            innerHandler: new GatedHandler(gate.Task) { InnerHandler = handlerA });
        using var clientB = new DefaultStellarSdkHttpClient("token-b", innerHandler: handlerB);
        using var serverA = new Server(HorizonUri, clientA);

        // Act
        var pageTask = serverA.Transactions.Execute();
        using var serverB = new Server(HorizonUri, clientB);
        _ = serverB.Transactions;
        gate.SetResult();
        var pageA = await pageTask;
        await pageA.NextPage()!;

        // Assert
        Assert.AreEqual(2, handlerA.CallCount);
        Assert.AreEqual(0, handlerB.CallCount);
    }

    /// <summary>
    ///     Verifies that NextPage and PreviousPage return <c>null</c> for a page without <c>_links</c>.
    /// </summary>
    [TestMethod]
    public void NextPageAndPreviousPage_PageWithoutLinks_ReturnNull()
    {
        // Arrange
        var page = JsonSerializer.Deserialize<Page<TransactionResponse>>("""{ "_embedded": { "records": [] } }""",
            JsonOptions.DefaultOptions)!;

        // Act & Assert
        Assert.IsNull(page.NextPage());
        Assert.IsNull(page.PreviousPage());
    }

    /// <summary>
    ///     Verifies that NextPage goes through the configured resilience pipeline: a transient 503 on the
    ///     followed page is retried instead of surfacing as a <see cref="Exceptions.ServiceUnavailableException" />.
    /// </summary>
    [TestMethod]
    public async Task NextPage_OnPageFromServer_RetriesThroughResiliencePipeline()
    {
        // Arrange
        var handler = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            ScriptedHttpMessageHandler.Ok(PageJson));
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(1),
            UseJitter = false,
        };
        resilienceOptions.RetryHttpStatusCodes.Add(HttpStatusCode.ServiceUnavailable);
        using var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions,
            innerHandler: handler);
        using var server = new Server(HorizonUri, httpClient);

        // Act
        var firstPage = await server.Transactions.Execute();
        var nextPage = await firstPage.NextPage()!;

        // Assert
        Assert.AreEqual(3, handler.CallCount);
        Assert.AreEqual(NextHref, handler.Requests[2].RequestUri!.ToString());
        Assert.IsNotNull(nextPage.Links);
    }

    /// <summary>
    ///     Verifies that a page reached through <see cref="Link{TResponse}.Follow(HttpClient)" /> keeps the client
    ///     it was followed with, so chained NextPage calls keep using it.
    /// </summary>
    [TestMethod]
    public async Task NextPage_OnPageFromLinkFollow_ReusesFollowedHttpClient()
    {
        // Arrange
        var handler = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Ok(PageJson),
            ScriptedHttpMessageHandler.Ok(PageJson));
        using var httpClient = new HttpClient(handler);
        var link = new Link<Page<TransactionResponse>> { Href = SelfHref };

        // Act
        var firstPage = await link.Follow(httpClient);
        var secondPage = await firstPage.NextPage()!;
        await secondPage.NextPage()!;

        // Assert
        Assert.AreEqual(3, handler.CallCount);
        Assert.AreEqual(SelfHref, handler.Requests[0].RequestUri!.ToString());
        Assert.AreEqual(NextHref, handler.Requests[1].RequestUri!.ToString());
        Assert.AreEqual(NextHref, handler.Requests[2].RequestUri!.ToString());
    }

    /// <summary>
    ///     Verifies that a page stays bound to the client that fetched it: once that client is disposed (as a
    ///     <see cref="Server" /> does with a client it created), NextPage throws instead of quietly following
    ///     with a new, unauthenticated client.
    /// </summary>
    [TestMethod]
    public async Task NextPage_AfterFetchingClientDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var handler = new ScriptedHttpMessageHandler(ScriptedHttpMessageHandler.Ok(PageJson));
        var httpClient = new HttpClient(handler);
        var link = new Link<Page<TransactionResponse>> { Href = SelfHref };
        var page = await link.Follow(httpClient);
        httpClient.Dispose();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => page.NextPage()!);
        Assert.AreEqual(1, handler.CallCount);
    }

    /// <summary>
    ///     Verifies that a page the SDK did not fetch (here, deserialized directly) has no client to reuse and
    ///     still follows its link with a new one. The link points at an unresolvable host, so the new client
    ///     fails with <see cref="HttpRequestException" />.
    /// </summary>
    [TestMethod]
    public async Task NextPage_OnPageNotFetchedBySdk_FollowsWithNewHttpClient()
    {
        // Arrange
        var page = JsonSerializer.Deserialize<Page<TransactionResponse>>(PageJson, JsonOptions.DefaultOptions)!;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => page.NextPage()!);
    }

    private static string PageJsonWithLinks(string nextHref, string prevHref)
    {
        return $$"""
                 {
                   "_links": {
                     "self": { "href": "{{SelfHref}}" },
                     "next": { "href": "{{nextHref}}" },
                     "prev": { "href": "{{prevHref}}" }
                   },
                   "_embedded": { "records": [] }
                 }
                 """;
    }

    /// <summary>Holds every request until <c>gate</c> completes, then passes it on.</summary>
    private sealed class GatedHandler(Task gate) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await gate;
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
