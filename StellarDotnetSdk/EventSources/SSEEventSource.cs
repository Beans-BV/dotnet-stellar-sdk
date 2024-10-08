using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using LaunchDarkly.EventSource;

namespace StellarDotnetSdk.EventSources;
#nullable disable

public class SseEventSource : IEventSource, IDisposable
{
    private readonly LaunchDarkly.EventSource.EventSource _eventSource;

    public SseEventSource(Uri uri, Action<ConfigurationBuilder> configureEventSource = null)
    {
        Url = uri;
        var config = new ConfigurationBuilder(uri);
        configureEventSource?.Invoke(config);
        _eventSource = new LaunchDarkly.EventSource.EventSource(config.Build());

        _eventSource.Opened += StateChangedEventHandler;
        _eventSource.Closed += StateChangedEventHandler;
        _eventSource.Error += (sender, args) =>
            Error?.Invoke(this, ConvertExceptionEventArgs(args));
        _eventSource.MessageReceived += MessageReceivedEventHandler;
    }

    [Obsolete] public NameValueCollection Headers { get; }

    public string LastEventId { get; private set; }

    [Obsolete] public string[] MessageTypes { get; }

    public EventSource.EventSourceState ReadyState { get; private set; }

    public int Timeout { get; set; }

    public Uri Url { get; set; }

    public event EventHandler<EventSource.ServerSentErrorEventArgs> Error;

    public event EventHandler<EventSource.ServerSentEventArgs> Message;

    public event EventHandler<EventSource.StateChangeEventArgs> StateChange;

    public async Task Connect()
    {
        await _eventSource.StartAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _eventSource?.Dispose();
    }

    public void Shutdown()
    {
        _eventSource.Close();
    }

    private void StateChangedEventHandler(object sender, StateChangedEventArgs args)
    {
        var newState = ConvertStateChangeEventArgs(args);
        ReadyState = newState.NewState;
        StateChange?.Invoke(this, newState);
    }

    private void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args)
    {
        if (args.EventName != "message")
        {
            return;
        }

        LastEventId = args.Message.LastEventId;

        Message?.Invoke(this, new EventSource.ServerSentEventArgs(args.Message.Data));
    }

    private static EventSource.StateChangeEventArgs ConvertStateChangeEventArgs(StateChangedEventArgs args)
    {
        var newState = ConvertEventSourceState(args.ReadyState);
        return new EventSource.StateChangeEventArgs { NewState = newState };
    }

    private static EventSource.EventSourceState ConvertEventSourceState(ReadyState state)
    {
        switch (state)
        {
            case LaunchDarkly.EventSource.ReadyState.Closed:
                return EventSource.EventSourceState.CLOSED;
            case LaunchDarkly.EventSource.ReadyState.Connecting:
                return EventSource.EventSourceState.CONNECTING;
            case LaunchDarkly.EventSource.ReadyState.Open:
                return EventSource.EventSourceState.OPEN;
            case LaunchDarkly.EventSource.ReadyState.Shutdown:
                return EventSource.EventSourceState.SHUTDOWN;
            case LaunchDarkly.EventSource.ReadyState.Raw:
            default:
                return EventSource.EventSourceState.RAW;
        }
    }

    private static EventSource.ServerSentErrorEventArgs ConvertExceptionEventArgs(ExceptionEventArgs args)
    {
        return new EventSource.ServerSentErrorEventArgs { Exception = args.Exception };
    }
}