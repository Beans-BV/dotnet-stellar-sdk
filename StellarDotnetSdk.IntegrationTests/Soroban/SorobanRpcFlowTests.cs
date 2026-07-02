using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses.SorobanRpc;
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

        // GetAccount (retries across the Horizon -> RPC ingestion handoff)
        var account = await CreateFundedAccountAsync();
        var rpcAccount = await GetRpcAccountWithRetryAsync(account.AccountId);
        rpcAccount.AccountId.Should().Be(account.AccountId);

        // Deploy an events-emitting contract so the GetEvents step below actually validates retrieval
        // (the hello_world contract emits no contract events).
        var contractId = await DeployContractAsync(account, ReadWasm("soroban_events_contract.wasm"));
        var invokeAccount = await GetRpcAccountWithRetryAsync(account.AccountId);
        var invokeTx = new TransactionBuilder(invokeAccount)
            .AddOperation(new InvokeContractOperation(contractId, "increment", null))
            .Build();

        // SimulateTransaction (assemble + sign here; assert its fields without a second round-trip).
        var sim = await SimulateAssembleSignAsync(invokeTx, account);
        sim.MinResourceFee.Should().NotBeNull();

        // SendTransaction + GetTransaction (poll)
        var final = await SendAndPollAsync(invokeTx);
        final.Status.Should().Be(TransactionInfo.TransactionStatus.SUCCESS);

        // GetLedgerEntries for the contract data entry
        var entries = await Rpc.GetLedgerEntries([CreateLedgerKeyContractData(contractId)]);
        entries.LedgerEntries.Should().NotBeNullOrEmpty();

        // GetEvents from the invoke ledger (where `increment` published its event) onward, filtered to
        // the contract — asserts an actual event was retrieved, not merely a non-null response.
        var request = new GetEventsRequest
        {
            StartLedger = final.Ledger,
            Filters = [new GetEventsRequest.EventFilter { ContractIds = [contractId] }],
        };
        var events = await Rpc.GetEvents(request);
        events.Events.Should().NotBeNullOrEmpty("the increment invoke should have published a contract event");
    }
}