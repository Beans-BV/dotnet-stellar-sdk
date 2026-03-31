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
    /// <summary>
    ///     Gets or sets the SSE event source used for streaming. If null, a default one is created on first stream.
    /// </summary>
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
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestBuilderStreamable{T, TResponse}" /> class.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="defaultSegment">The default URL path segment for this endpoint.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    public RequestBuilderStreamable(Uri serverUri, string defaultSegment, HttpClient httpClient)
        : base(serverUri, defaultSegment, httpClient)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestBuilderStreamable{T, TResponse}" /> class with a custom event source.
    /// </summary>
    /// <param name="serverUri">The base Horizon server URI.</param>
    /// <param name="defaultSegment">The default URL path segment for this endpoint.</param>
    /// <param name="httpClient">The HTTP client used for sending requests.</param>
    /// <param name="eventSource">A custom SSE event source implementation.</param>
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