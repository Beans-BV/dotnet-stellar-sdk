using System;

namespace StellarDotnetSdk.Tests.Requests;

public class StreamAction
{
    internal readonly string? Content;
    internal readonly TimeSpan Delay;

    private StreamAction(TimeSpan delay, string? content)
    {
        Delay = delay;
        Content = content;
    }

    public bool ShouldQuit()
    {
        return Content == null;
    }

    public static StreamAction Write(string content)
    {
        return new StreamAction(TimeSpan.Zero, content);
    }

    public static StreamAction CloseStream()
    {
        return new StreamAction(TimeSpan.Zero, null);
    }

    public StreamAction AfterDelay(TimeSpan delay)
    {
        return new StreamAction(delay, Content);
    }
}