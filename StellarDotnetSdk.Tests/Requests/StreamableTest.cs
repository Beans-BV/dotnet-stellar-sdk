using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.EventSources;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Helper class for testing streamable request builders with SSE event sources.
/// </summary>
/// <typeparam name="T">The type of response object expected from the stream.</typeparam>
public class StreamableTest<T> where T : class
{
    private readonly string _eventId;
    private readonly string _json;
    private readonly Action<T> _testAction;
    private SseEventSource? _eventSource;
    private FakeStreamableRequestBuilder? _requestBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamableTest{T}" /> class.
    /// </summary>
    /// <param name="json">The JSON data to stream.</param>
    /// <param name="action">The action to perform when a message is received.</param>
    /// <param name="eventId">Optional event ID for the streamed message.</param>
    public StreamableTest(string json, Action<T> action, string? eventId = null)
    {
        _json = json.Replace("\r\n", "").Replace("\n", "");
        _testAction = action;
        _eventSource = null;
        _requestBuilder = null;
        _eventId = eventId ?? "1234";
    }

    /// <summary>
    ///     Gets the last event ID received from the stream.
    /// </summary>
    public string? LastEventId => _eventSource?.LastEventId;

    /// <summary>
    ///     Gets the URI that was built for the request.
    /// </summary>
    public string? Uri => _requestBuilder?.BuildUri().ToString();

    /// <summary>
    ///     Runs the streamable test, connecting to the event source and verifying the message is received and processed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown when the task does not complete or no message is received.</exception>
    public async Task Run()
    {
        var fakeHandler = new FakeStreamableHttpMessageHandler();
        var stream = $"event: open\ndata: hello\n\nid: {_eventId}\ndata: {_json}\n\n";
        fakeHandler.QueueResponse(FakeResponse.StartsStream(StreamAction.Write(stream)));

        _eventSource = new SseEventSource(new Uri("http://test.com"),
            builder => builder.MessageHandler(fakeHandler));

        _requestBuilder = new FakeStreamableRequestBuilder(new Uri("https://horizon-testnet.stellar.org"), "test",
            null, _eventSource);

        Exception? handlerException = null;
        var messageReceived = false;
        var handler = new EventHandler<T>((sender, e) =>
        {
            messageReceived = true;
            try
            {
                _testAction(e);
            }
            catch (Exception ex)
            {
                handlerException = ex;
            }
            finally
            {
                _eventSource.Shutdown();
            }
        });
        var task = _requestBuilder.Stream(handler).Connect();
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5.0));
        var completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);
        if (completedTask != task)
        {
            throw new Exception("Task did not complete.");
        }

        if (!messageReceived)
        {
            throw new Exception("No message was received from the stream.");
        }

        if (handlerException != null)
        {
            throw handlerException;
        }
    }

    /// <summary>
    ///     Fake streamable request builder for testing purposes.
    /// </summary>
    public class FakeStreamableRequestBuilder : RequestBuilderStreamable<FakeStreamableRequestBuilder, T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeStreamableRequestBuilder" /> class.
        /// </summary>
        /// <param name="serverUri">The server URI.</param>
        /// <param name="defaultSegment">The default segment for the request.</param>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="eventSource">The event source for streaming.</param>
        public FakeStreamableRequestBuilder(Uri serverUri, string defaultSegment, HttpClient httpClient,
            IEventSource eventSource)
            : base(serverUri, defaultSegment, httpClient, eventSource)
        {
        }
    }
}