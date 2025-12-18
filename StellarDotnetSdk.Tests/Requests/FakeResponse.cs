using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Abstract base class for creating fake HTTP responses for testing purposes.
/// </summary>
public abstract class FakeResponse
{
    /// <summary>
    /// Creates an HTTP response message.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An HTTP response message.</returns>
    public abstract HttpResponseMessage MakeResponse(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a fake response that starts a streaming response with the specified actions.
    /// </summary>
    /// <param name="actions">The stream actions to execute.</param>
    /// <returns>A fake response configured for streaming.</returns>
    public static FakeResponse StartsStream(params StreamAction[] actions)
    {
        return new FakeResponseWithStream(actions);
    }

    /// <summary>
    /// Creates a fake response that throws an IO error.
    /// </summary>
    /// <returns>A fake response configured to throw an IO error.</returns>
    public static FakeResponse WithIOError()
    {
        return new FakeResponseWithIOError();
    }
}

internal class FakeResponseWithIOError : FakeResponse
{
    public override HttpResponseMessage MakeResponse(CancellationToken cancellationToken)
    {
        throw new HttpRequestException("Unit Test Exception Message");
    }
}

internal class FakeResponseWithStream : FakeResponse
{
    private readonly StreamAction[] _actions;

    public FakeResponseWithStream(StreamAction[] actions)
    {
        _actions = actions;
    }

    public override HttpResponseMessage MakeResponse(CancellationToken cancellationToken)
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var streamRead = new AnonymousPipeServerStream(PipeDirection.In);
        var streamWrite = new AnonymousPipeClientStream(PipeDirection.Out, streamRead.ClientSafePipeHandle);
        var content = new StreamContent(streamRead);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
        httpResponse.Content = content;

        Task.Run(() => WriteStreamingResponse(streamWrite, cancellationToken), cancellationToken);

        return httpResponse;
    }

    private async Task WriteStreamingResponse(Stream output, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var action in _actions)
            {
                if (action.Delay != TimeSpan.Zero)
                {
                    await Task.Delay(action.Delay, cancellationToken);
                }

                if (action.ShouldQuit())
                {
                    return;
                }

                Assert.IsNotNull(action.Content);
                var data = Encoding.UTF8.GetBytes(action.Content);
                await output.WriteAsync(data, cancellationToken);
            }

            // if we've run out of actions, leave the stream open until it's cancelled
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        catch (Exception)
        {
            // just exit
        }
    }
}