using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Fake HTTP message handler for testing streamable requests. Queues responses that are returned in order.
/// </summary>
public class FakeStreamableHttpMessageHandler : HttpMessageHandler
{
    // Requests that were sent via the handler
    private readonly Queue<FakeResponse> _responses = new();

    /// <summary>
    ///     Sends an HTTP request and returns a queued fake response.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no response has been queued.</exception>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No response configured");
        }

        // RequestReceived?.Invoke(this, request);

        var response = _responses.Dequeue();
        return Task.FromResult(response.MakeResponse(cancellationToken));
    }

    /// <summary>
    ///     Queues a fake response to be returned on the next request.
    /// </summary>
    /// <param name="response">The fake response to queue.</param>
    public void QueueResponse(FakeResponse response)
    {
        _responses.Enqueue(response);
    }
}