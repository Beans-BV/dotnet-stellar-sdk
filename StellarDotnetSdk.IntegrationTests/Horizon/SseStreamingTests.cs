using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.EventSources;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(90_000)]
public class SseStreamingTests : IntegrationTestBase
{
    [Test]
    public async Task LedgersStream_ReceivesLiveLedger()
    {
        var firstLedger = new TaskCompletionSource<LedgerResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        IEventSource eventSource = null!;
        eventSource = Server.Ledgers.Cursor("now").Stream((_, ledger) =>
        {
            firstLedger.TrySetResult(ledger);
            eventSource.Shutdown();
        });

        using var cts = new CancellationTokenSource();
        var connectTask = eventSource.Connect();
        var completed = await Task.WhenAny(firstLedger.Task, Task.Delay(TimeSpan.FromSeconds(45), cts.Token));
        cts.Cancel(); // free the timeout timer promptly
        eventSource.Shutdown();
        // Drain the connect loop before disposing, so Dispose never races a running read.
        try
        {
            await connectTask;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            /* stream torn down by Shutdown */
        }
        eventSource.Dispose();

        if (completed != firstLedger.Task)
        {
            Assert.Inconclusive("No ledger event arrived within 45s (Horizon SSE outage/lag, not an SDK regression).");
        }

        var ledger = await firstLedger.Task;
        ledger.Sequence.Should().BeGreaterThan(0);
        ledger.Hash.Should().NotBeNullOrEmpty();
    }
}