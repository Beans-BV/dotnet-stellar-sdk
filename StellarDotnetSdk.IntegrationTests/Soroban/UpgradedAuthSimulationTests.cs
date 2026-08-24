using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Soroban;

/// <summary>
///     End-to-end coverage for the <c>useUpgradedAuth</c> simulation flag (CAP-71 v2 authorization credentials).
///     This is the only place the SDK's v2 credential decoding and its address-bound signature preimage are
///     exercised against a live host, rather than against entries the SDK encoded itself.
/// </summary>
[TestFixture]
[CancelAfter(300_000)]
public class UpgradedAuthSimulationTests : SorobanIntegrationTestBase
{
    /// <summary>
    ///     Simulates one transaction twice — once without the flag, once with it — to show that the flag is what
    ///     turns the recorded credential from <see cref="SorobanAddressCredentials" />
    ///     (<c>SOROBAN_CREDENTIALS_ADDRESS</c>) into <see cref="SorobanAddressCredentialsV2" />
    ///     (<c>SOROBAN_CREDENTIALS_ADDRESS_V2</c>), then signs the v2 entry and submits it. Core accepting the
    ///     transaction is what proves the SDK signs v2 over the right preimage
    ///     (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c>); a v1 preimage would produce a signature the
    ///     host rejects, and nothing local would catch it.
    /// </summary>
    [Test]
    public async Task SimulateTransaction_WithUseUpgradedAuth_RecordsAndSignsV2AddressCredentials()
    {
        var source = await CreateFundedAccountAsync();
        var authorizer = await CreateFundedAccountAsync();
        var wasmHash = await UploadWasmAsync(source, ReadWasm("soroban_hello_world_contract.wasm"));

        // Deploying a contract from an address other than the transaction source is what makes recording-mode
        // simulation emit an *address* credential: the host requires that address to authorize the create.
        // Deploying from the source account would yield source-account credentials, which carry no variant and
        // would make the assertions below vacuous.
        var rpcAccount = await GetRpcAccountWithRetryAsync(source.AccountId);
        var tx = new TransactionBuilder(rpcAccount)
            .AddOperation(CreateContractOperation.FromAddress(wasmHash, authorizer.AccountId))
            .Build();

        // Control: the same transaction simulated without the flag must still record legacy v1 credentials.
        // Without this half, a v2 result below would not prove the flag did anything.
        var legacySim = await Rpc.SimulateTransaction(tx, null, AuthMode.RECORD);
        AssertSimulated(legacySim);
        if (legacySim.SorobanAuthorization![0].Credentials is SorobanAddressCredentialsV2)
        {
            Assert.Inconclusive(
                "Stellar RPC returned ADDRESS_V2 credentials without useUpgradedAuth, i.e. it has flipped its " +
                "server-side default (planned for protocol 29), making the flag a no-op. That is an ecosystem " +
                "change, not an SDK regression.");
        }
        legacySim.SorobanAuthorization[0].Credentials.Should().BeOfType<SorobanAddressCredentials>();

        // With the flag, the identical transaction records CAP-71 address-bound credentials instead.
        var upgradedSim = await Rpc.SimulateTransaction(tx, null, AuthMode.RECORD, true);
        AssertSimulated(upgradedSim);
        var entry = upgradedSim.SorobanAuthorization![0];
        var credentials = entry.Credentials.Should()
            .BeOfType<SorobanAddressCredentialsV2>(
                "useUpgradedAuth: true asks Stellar RPC for SOROBAN_CREDENTIALS_ADDRESS_V2")
            .Subject;
        credentials.Address.Should().BeOfType<ScAccountId>()
            .Which.InnerValue.Should().Be(authorizer.AccountId);

        // Sign the recorded v2 entry as the authorizer and submit. Recording mode returns entries unsigned
        // (no expiration, void signature), so the SDK supplies both here.
        var latest = await Rpc.GetLatestLedger();
        var signedEntry = SorobanAuthorization.AuthorizeEntry(
            entry,
            authorizer,
            (uint)latest.Sequence + 100,
            Network.Test());
        signedEntry.Credentials.Should().BeOfType<SorobanAddressCredentialsV2>(
            "signing must preserve the credential variant simulation returned");

        tx.SetSorobanTransactionData(upgradedSim.SorobanTransactionData!);
        tx.SetSorobanAuthorization([signedEntry]);
        // The signed authorization entry is larger than the placeholder simulation measured, so top up the
        // resource fee rather than sending exactly MinResourceFee.
        tx.AddResourceFee((upgradedSim.MinResourceFee ?? 0) + 100_000);
        tx.Sign(source);

        var result = await SendAndPollAsync(tx);
        result.Status.Should().Be(TransactionInfo.TransactionStatus.SUCCESS);
        result.CreatedContractId.Should().NotBeNull(
            "the create should have run, which it only does if the host accepted the v2 authorization signature");
    }

    private static void AssertSimulated(SimulateTransactionResponse simulation)
    {
        simulation.Error.Should().BeNull("simulation should not error: {0}", simulation.Error);
        simulation.SorobanAuthorization.Should()
            .NotBeNullOrEmpty("deploying from a non-source address should record an authorization entry");
    }
}
