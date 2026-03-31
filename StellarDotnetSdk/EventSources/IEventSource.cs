using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace StellarDotnetSdk.EventSources;

/// <summary>
/// Defines the contract for a Server-Sent Events (SSE) event source used to stream events from a Stellar Horizon server.
/// </summary>
public interface IEventSource
{
    /// <summary>
    /// Gets the HTTP headers sent with the event source request.
    /// </summary>
    NameValueCollection Headers { get; }

    /// <summary>
    /// Gets the ID of the last event received from the server, used for reconnection.
    /// </summary>
    string LastEventId { get; }

    /// <summary>
    /// Gets the types of messages this event source is configured to handle.
    /// </summary>
    string[] MessageTypes { get; }

    /// <summary>
    /// Gets the current connection state of the event source.
    /// </summary>
    EventSource.EventSourceState ReadyState { get; }

    /// <summary>
    /// Gets or sets the connection timeout in milliseconds.
    /// </summary>
    int Timeout { get; set; }

    /// <summary>
    /// Gets or sets the URI of the SSE endpoint.
    /// </summary>
    Uri Url { get; set; }

    /// <summary>
    /// Occurs when an error is encountered during the SSE connection or while processing events.
    /// </summary>
    event EventHandler<EventSource.ServerSentErrorEventArgs> Error;

    /// <summary>
    /// Occurs when a new message is received from the SSE endpoint.
    /// </summary>
    event EventHandler<EventSource.ServerSentEventArgs> Message;

    /// <summary>
    /// Occurs when the connection state of the event source changes.
    /// </summary>
    event EventHandler<EventSource.StateChangeEventArgs> StateChange;

    /// <summary>
    /// Opens a connection to the SSE endpoint and begins listening for events.
    /// </summary>
    /// <returns>A task that represents the asynchronous connect operation.</returns>
    Task Connect();

    /// <summary>
    /// Releases all resources used by the event source.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Gracefully shuts down the event source connection.
    /// </summary>
    void Shutdown();
}