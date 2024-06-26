﻿using System;
using System.Net.Http;
using StellarDotnetSdk.EventSources;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

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
            var responseObject = JsonSingleton.GetInstance<TResponse>(e.Data) ??
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