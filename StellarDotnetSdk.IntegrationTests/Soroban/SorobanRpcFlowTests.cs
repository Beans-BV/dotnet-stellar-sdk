using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Soroban;

[TestFixture]
[CancelAfter(300_000)]
public class SorobanRpcFlowTests : SorobanIntegrationTestBase
{
    [Test]
    public async Task RpcFlow_HealthLedgerAccountSimulateSendGetEntriesEvents()
    {
        // GetHealth
        var health = await Rpc.GetHealth();
        health.Status.Should().Be("healthy");

        // GetLatestLedger
        var latest = await Rpc.GetLatestLedger();
        latest.Sequence.Should().BeGreaterThan(0);

        // GetAccount (for a funded account)
        var account = await CreateFundedAccountAsync();
        var rpcAccount = await Rpc.GetAccount(account.AccountId);
        rpcAccount.AccountId.Should().Be(account.AccountId);

        // Deploy + invoke drives Simulate -> Send -> GetTransaction internally.
        var contractId = await DeployHelloWorldAsync(account);
        var invokeAccount = await Rpc.GetAccount(account.AccountId);
        var invokeTx = new TransactionBuilder(invokeAccount)
            .AddOperation(new InvokeContractOperation(contractId, "hello", [new SCSymbol("rpc")]))
            .Build();

        // SimulateTransaction (explicit, to assert its fields)
        var sim = await Rpc.SimulateTransaction(invokeTx);
        sim.Error.Should().BeNull();
        sim.MinResourceFee.Should().NotBeNull();

        // SendTransaction + GetTransaction (poll)
        await SimulateAssembleSignAsync(invokeTx, account);
        var final = await SendAndPollAsync(invokeTx);
        final.Status.Should().Be(TransactionInfo.TransactionStatus.SUCCESS);

        // GetLedgerEntries for the contract data entry
        var entries = await Rpc.GetLedgerEntries([CreateLedgerKeyContractData(contractId)]);
        entries.LedgerEntries.Should().NotBeNullOrEmpty();

        // GetEvents from the ledger the contract was created in onward
        var request = new GetEventsRequest
        {
            StartLedger = final.Ledger,
            Filters = [new GetEventsRequest.EventFilter { ContractIds = [contractId] }],
        };
        var events = await Rpc.GetEvents(request);
        events.Should().NotBeNull();
        events.LatestLedger.Should().NotBeNull();
    }
}