using System;
using System.Net.Http;
using System.Text.Json;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.EventSources;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Defines the ability to open a Server-Sent Events (SSE) stream from a Horizon endpoint.
/// </summary>
/// <typeparam name="TResponse">The type of response objects received from the event stream.</typeparam>
public interface IRequestBuilderStreamable<TResponse> where TResponse : class
{
    IEventSource? EventSource { get; set; }

    /// <Summary>
    ///     Allows to stream SSE events from horizon.
    ///     Certain endpoints in Horizon can be called in streaming mode using Server-Sent Events.
    ///     This mode will keep the connection to horizon open and horizon will continue to return
    ///     responses as ledgers close.
    ///     <a href="http://www.w3.org/TR/eventsource/" target="_blank">Server-Sent Events</a>
    ///     <a href="https://www.stellar.org/developers/horizon/learn/responses.html" target="_blank">
    ///         Response Format
    ///         documentation
    ///     </a>
    /// </Summary>
    /// <param name="listener">EventListener implementation with EffectResponse type</param>
    /// <returns>EventSource object, so you can <code>close()</code> connection when not needed anymore</returns>
    IEventSource Stream(EventHandler<TResponse> listener);
}

/// <summary>
///     Base class for request builders that support both paginated execution and Server-Sent Events (SSE) streaming.
/// </summary>
/// <typeparam name="T">The concrete request builder type (for fluent chaining).</typeparam>
/// <typeparam name="TResponse">The response element type for both paged results and streamed events.</typeparam>
public class RequestBuilderStreamable<T, TResponse>
    : RequestBuilderExecutePageable<T, TResponse>, IRequestBuilderStreamable<TResponse>
    where T : class where TResponse : class
{
    public RequestBuilderStreamable(Uri serverUri, string defaultSegment, HttpClient httpClient)
        : base(serverUri, defaultSegment, httpClient)
    {
    }

    public RequestBuilderStreamable(Uri serverUri, string defaultSegment, HttpClient httpClient,
        IEventSource eventSource)
        : base(serverUri, defaultSegment, httpClient)
    {
        EventSource = eventSource;
    }

    /// <inheritdoc />
    public IEventSource? EventSource { get; set; }

    /// <inheritdoc />
    public IEventSource Stream(EventHandler<TResponse> listener)
    {
        EventSource ??= new SseEventSource(BuildUri());

        EventSource.Message += (_, e) =>
        {
            var responseObject = JsonSerializer.Deserialize<TResponse>(e.Data, JsonOptions.DefaultOptions) ??
                                 throw new NotSupportedException("Unknown response type");

            if (responseObject is IPagingToken page)
            {
                Cursor(page.PagingToken);
                EventSource.Url = BuildUri();
            }

            listener.Invoke(this, responseObject);
        };

        return EventSource;
    }
}