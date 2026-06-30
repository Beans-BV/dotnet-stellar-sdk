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
        // Extend to (remaining TTL + 100k) ledgers from now, guaranteeing the new liveUntil exceeds the old.
        // (long) cast avoids any uint underflow if the ledger raced past oldLiveUntil between the two reads.
        var extendTo = (uint)((long)oldLiveUntil - currentLedger + 100_000);

        var rpcAccount = await Rpc.GetAccount(account.AccountId);
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

        var rpcAccount = await Rpc.GetAccount(account.AccountId);
        var tx = new TransactionBuilder(rpcAccount)
            .AddOperation(new RestoreFootprintOperation())
            .Build();
        var footprint = new LedgerFootprint { ReadWrite = [key] };
        tx.SetSorobanTransactionData(new SorobanTransactionData(new SorobanResources(footprint, 0, 0, 0), 0));

        var sim = await Rpc.SimulateTransaction(tx);
        // The RPC accepted and processed a well-formed RestoreFootprintOp. Restoring a live (non-archived)
        // entry is a no-op, so we assert the SDK/RPC round-trip simulated without error rather than
        // on-chain restoration.
        sim.Error.Should().BeNull("RestoreFootprintOp should simulate without error: {0}", sim.Error);
    }
}