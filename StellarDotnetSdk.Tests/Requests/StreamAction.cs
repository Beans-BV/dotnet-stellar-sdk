using System;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Represents an action to perform on a streaming response for testing purposes.
/// </summary>
public class StreamAction
{
    internal readonly string? Content;
    internal readonly TimeSpan Delay;

    private StreamAction(TimeSpan delay, string? content)
    {
        Delay = delay;
        Content = content;
    }

    /// <summary>
    ///     Determines whether the stream should be closed.
    /// </summary>
    /// <returns>True if the stream should be closed; otherwise, false.</returns>
    public bool ShouldQuit()
    {
        return Content == null;
    }

    /// <summary>
    ///     Creates a stream action that writes the specified content immediately.
    /// </summary>
    /// <param name="content">The content to write to the stream.</param>
    /// <returns>A stream action configured to write the content.</returns>
    public static StreamAction Write(string content)
    {
        return new StreamAction(TimeSpan.Zero, content);
    }

    /// <summary>
    ///     Creates a stream action that closes the stream.
    /// </summary>
    /// <returns>A stream action configured to close the stream.</returns>
    public static StreamAction CloseStream()
    {
        return new StreamAction(TimeSpan.Zero, null);
    }

    /// <summary>
    ///     Creates a new stream action with the same content but with an additional delay.
    /// </summary>
    /// <param name="delay">The delay to add before executing the action.</param>
    /// <returns>A new stream action with the specified delay.</returns>
    public StreamAction AfterDelay(TimeSpan delay)
    {
        return new StreamAction(delay, Content);
    }
}