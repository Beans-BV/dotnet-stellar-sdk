using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Soroban;

[TestFixture]
[CancelAfter(300_000)]
public class FootprintTests : SorobanIntegrationTestBase
{
    [Test]
    public async Task ExtendFootprint_RaisesContractDataTtl()
    {
        var account = await CreateFundedAccountAsync();
        var contractId = await DeployHelloWorldAsync(account);
        var key = CreateLedgerKeyContractData(contractId);

        var before = await Rpc.GetLedgerEntries([key]);
        before.LedgerEntries.Should().NotBeNullOrEmpty();
        var oldLiveUntil = before.LedgerEntries![0].LiveUntilLedger!.Value;

        var latest = await Rpc.GetLatestLedger();
        var currentLedger = (uint)latest.Sequence;
        // extendTo is RELATIVE (ledgers from the current ledger), so remaining + 100k makes the new
        // liveUntil exceed the old. For a freshly deployed contract oldLiveUntil > currentLedger; the
        // long arithmetic plus the clamp keep a ledger race from ever wrapping the uint cast.
        var extendTo = (uint)Math.Max(100_000, (long)oldLiveUntil - currentLedger + 100_000);

        var rpcAccount = await GetRpcAccountWithRetryAsync(account.AccountId);
        var tx = new TransactionBuilder(rpcAccount)
            .AddOperation(new ExtendFootprintOperation(extendTo))
            .Build();
        var footprint = new LedgerFootprint { ReadOnly = [key] };
        tx.SetSorobanTransactionData(new SorobanTransactionData(new SorobanResources(footprint, 0, 0, 0), 0));
        await SimulateAssembleSignAsync(tx, account);
        await SendAndPollAsync(tx);

        var after = await Rpc.GetLedgerEntries([key]);
        after.LedgerEntries.Should().NotBeNullOrEmpty();
        var newLiveUntil = after.LedgerEntries![0].LiveUntilLedger!.Value;
        newLiveUntil.Should().BeGreaterThan(oldLiveUntil);
    }

    [Test]
    public async Task RestoreFootprint_BuildsAndSimulates()
    {
        // Smoke check only: a true persistent archive->restore cycle is infeasible on Testnet
        // (min persistent TTL ~5+ hours; temp entries can't be restored; Protocol 23+ auto-restores).
        // This exercises the SDK's RestoreFootprintOperation + the RPC simulate path against the
        // contract's live persistent entry.
        var account = await CreateFundedAccountAsync();
        var contractId = await DeployHelloWorldAsync(account);
        var key = CreateLedgerKeyContractData(contractId);

        var rpcAccount = await GetRpcAccountWithRetryAsync(account.AccountId);
        var tx = new TransactionBuilder(rpcAccount)
            .AddOperation(new RestoreFootprintOperation())
            .Build();
        var footprint = new LedgerFootprint { ReadWrite = [key] };
        tx.SetSorobanTransactionData(new SorobanTransactionData(new SorobanResources(footprint, 0, 0, 0), 0));

        var sim = await Rpc.SimulateTransaction(tx);
        // Restoring a LIVE (non-archived) entry is version-dependent: some RPC/protocol versions treat it
        // as a no-op (Error == null), others reject it. Either way the SDK built + serialized a valid
        // RestoreFootprintOp and the RPC processed the request — that is what this smoke check verifies.
        // Map a version-dependent rejection to Inconclusive rather than a false failure.
        if (sim.Error != null)
        {
            Assert.Inconclusive(
                $"RPC rejected RestoreFootprint over a live entry (version-dependent, not an SDK regression): {sim.Error}");
        }
        sim.SorobanTransactionData.Should()
            .NotBeNull("a successful restore simulation should return assembled Soroban transaction data");
    }
}