using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.EventSources;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for streamable request builders and SSE event source functionality.
/// </summary>
[TestClass]
public class RequestBuilderStreamableTest
{
    private readonly Uri _uri = new("https://test.com");

    /// <summary>
    /// Verifies that SseEventSource skips the first "hello" message and processes subsequent data messages.
    /// </summary>
    [TestMethod]
    public async Task Connect_WithHelloMessage_SkipsHelloAndReceivesSubsequentData()
    {
        // Arrange
        // Check we skip the first message with "hello" data
        var fakeHandler = new FakeStreamableHttpMessageHandler();
        var stream = "event: open\ndata: hello\n\ndata: foobar\n\n";
        fakeHandler.QueueResponse(FakeResponse.StartsStream(StreamAction.Write(stream)));

        using var eventSource = new SseEventSource(_uri, builder => builder.MessageHandler(fakeHandler));
        string? dataReceived = null;
        eventSource.Message += (sender, args) =>
        {
            dataReceived = args.Data;
            eventSource.Shutdown();
        };

        // Act
        await eventSource.Connect();

        // Assert
        Assert.AreEqual("foobar", dataReceived);
    }

    /// <summary>
    /// Verifies that SseEventSource handles multiple error events and retries connection.
    /// </summary>
    [TestMethod]
    public async Task Connect_WithMultipleErrors_RetriesAndCountsErrors()
    {
        // Arrange
        var fakeHandler = new FakeStreamableHttpMessageHandler();
        fakeHandler.QueueResponse(FakeResponse.WithIOError());
        fakeHandler.QueueResponse(FakeResponse.WithIOError());
        fakeHandler.QueueResponse(FakeResponse.StartsStream());

        using var eventSource = new SseEventSource(_uri, builder => builder.MessageHandler(fakeHandler));
        var errorCount = 0;
        eventSource.Error += (sender, args) =>
        {
            errorCount += 1;
            if (errorCount >= 2)
            {
                eventSource.Shutdown();
            }
        };

        // Act
        await eventSource.Connect();

        // Assert
        Assert.AreEqual(2, errorCount);
    }
}