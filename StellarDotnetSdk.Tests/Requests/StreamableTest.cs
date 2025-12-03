using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.EventSources;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

public class StreamableTest<T> where T : class
{
    private readonly string _eventId;
    private readonly string _json;
    private readonly Action<T> _testAction;
    private SseEventSource? _eventSource;
    private FakeStreamableRequestBuilder? _requestBuilder;

    public StreamableTest(string json, Action<T> action, string? eventId = null)
    {
        _json = json.Replace("\r\n", "").Replace("\n", "");
        _testAction = action;
        _eventSource = null;
        _requestBuilder = null;
        _eventId = eventId ?? "1234";
    }

    public string? LastEventId => _eventSource?.LastEventId;
    public string? Uri => _requestBuilder?.BuildUri().ToString();

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

    public class FakeStreamableRequestBuilder : RequestBuilderStreamable<FakeStreamableRequestBuilder, T>
    {
        public FakeStreamableRequestBuilder(Uri serverUri, string defaultSegment, HttpClient httpClient,
            IEventSource eventSource)
            : base(serverUri, defaultSegment, httpClient, eventSource)
        {
        }
    }
}