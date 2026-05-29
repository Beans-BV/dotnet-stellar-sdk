using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Test helper that returns a scripted sequence of responses for each call to
///     <see cref="SendAsync" />. Tracks the request method and full request count so tests
///     can assert on retry behavior.
/// </summary>
internal sealed class ScriptedHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses;

    public ScriptedHttpMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
    {
        _responses = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>(responses);
    }

    public int CallCount { get; private set; }

    public List<HttpRequestMessage> Requests { get; } = new();

    public static Func<HttpRequestMessage, HttpResponseMessage> Status(HttpStatusCode code, string? retryAfter = null)
    {
        return _ =>
        {
            var resp = new HttpResponseMessage(code) { Content = new StringContent("{}") };
            if (retryAfter != null)
            {
                resp.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
            }

            return resp;
        };
    }

    public static Func<HttpRequestMessage, HttpResponseMessage> Ok(string body = "{}")
    {
        return _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        CallCount++;
        Requests.Add(request);
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException($"No more scripted responses (call #{CallCount})");
        }

        return Task.FromResult(_responses.Dequeue()(request));
    }
}