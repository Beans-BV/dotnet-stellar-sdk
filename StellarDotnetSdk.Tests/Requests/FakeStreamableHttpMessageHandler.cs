using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Tests.Requests;

public class FakeStreamableHttpMessageHandler : HttpMessageHandler
{
    // Requests that were sent via the handler
    private readonly Queue<FakeResponse> _responses = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_responses.Count == 0)
            throw new InvalidOperationException("No response configured");

        // RequestReceived?.Invoke(this, request);

        var response = _responses.Dequeue();
        return Task.FromResult(response.MakeResponse(cancellationToken));
    }

    public void QueueResponse(FakeResponse response)
    {
        _responses.Enqueue(response);
    }
}