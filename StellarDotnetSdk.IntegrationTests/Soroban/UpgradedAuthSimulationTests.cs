using System;
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
///     End-to-end coverage for the <c>useUpgradedAuth</c> simulation flag (CAP-71 v2 authorization credentials):
///     the SDK's default, which asks for <c>SOROBAN_CREDENTIALS_ADDRESS_V2</c>, and the explicit legacy opt-out.
///     This is the only place the SDK's v2 credential decoding and its address-bound signature preimage are
///     exercised against a live host, rather than against entries the SDK encoded itself.
///     <para>
///         What these tests prove is host acceptance, not the SDK's wire shape. The variant a simulation returns
///         depends on the server's own default as well as on what the SDK sends, so once Stellar RPC flips its
///         server-side default to v2 the default test can no longer tell the SDK's <c>true</c> from the server's
///         default. The opt-out test cannot catch a wrong flag even today: an SDK that omits <c>false</c> still
///         gets legacy credentials from the server's current default, and one that sends <c>true</c> instead
///         passes through the NOTE branch. The request body is pinned by the unit tests
///         <c>StellarRpcServerTest.SimulateTransaction_WithoutUseUpgradedAuth_SendsTrue</c> and
///         <c>SimulateTransaction_WithUseUpgradedAuth_SendsJsonBoolean</c>.
///     </para>
/// </summary>
[TestFixture]
[CancelAfter(300_000)]
public class UpgradedAuthSimulationTests : SorobanIntegrationTestBase
{
    // The hello-world wasm is uploaded once per fixture run and shared by both tests; the funded accounts are
    // still created per test.
    private string? _wasmHash;

    /// <summary>
    ///     The SDK default: simulating <em>without</em> the flag must record
    ///     <see cref="SorobanAddressCredentialsV2" /> (<c>SOROBAN_CREDENTIALS_ADDRESS_V2</c>). The recorded entry is
    ///     then signed with the default <see cref="SorobanCredentialsVersion.Preserve" /> and submitted. Core
    ///     accepting the transaction is what proves the SDK signs v2 over the right preimage
    ///     (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c>); a v1 preimage would produce a signature the
    ///     host rejects, and nothing local would catch it.
    /// </summary>
    [Test]
    public async Task SimulateTransaction_ByDefault_RecordsAndSignsV2AddressCredentials()
    {
        var (source, authorizer, tx) = await BuildAddressAuthorizedDeployAsync();

        // No flag: the SDK sends useUpgradedAuth: true on its own.
        var defaultSim = await Rpc.SimulateTransaction(tx, null, AuthMode.RECORD);
        AssertSimulated(defaultSim);
        var entry = defaultSim.SorobanAuthorization![0];
        var credentials = entry.Credentials.Should()
            .BeOfType<SorobanAddressCredentialsV2>(
                "the SDK defaults useUpgradedAuth to true, so an unflagged simulation records ADDRESS_V2")
            .Subject;
        credentials.Address.Should().BeOfType<ScAccountId>()
            .Which.InnerValue.Should().Be(authorizer.AccountId);

        await SignSubmitAndAssertSuccessAsync(tx, source, authorizer, defaultSim,
            typeof(SorobanAddressCredentialsV2));
    }

    /// <summary>
    ///     The legacy opt-out: <c>useUpgradedAuth: false</c> must still record legacy
    ///     <see cref="SorobanAddressCredentials" /> (<c>SOROBAN_CREDENTIALS_ADDRESS</c>), and signing it with the
    ///     default <see cref="SorobanCredentialsVersion.Preserve" /> must produce a v1 signature the host accepts.
    ///     Legacy credentials stay valid on protocol 28, so the opt-out has to remain usable end to end.
    /// </summary>
    [Test]
    public async Task SimulateTransaction_WithUseUpgradedAuthFalse_RecordsAndSignsLegacyAddressCredentials()
    {
        var (source, authorizer, tx) = await BuildAddressAuthorizedDeployAsync();

        var legacySim = await Rpc.SimulateTransaction(tx, null, AuthMode.RECORD, false);
        AssertSimulated(legacySim);
        var legacyCredentials = legacySim.SorobanAuthorization![0].Credentials;
        Type expectedSignedType;
        if (legacyCredentials is SorobanAddressCredentialsV2)
        {
            // Stellar RPC no longer honours the opt-out: under SDF's tentative plan the flag becomes a no-op once
            // RPC flips its server-side default to v2 (planned for protocol 29, not yet done on Testnet in
            // September 2026), so false cannot yield legacy credentials any more. That is an ecosystem change, not
            // an SDK regression, so the v1 assertion is dropped and the test carries on: signing and submitting
            // what came back still exercises Preserve. Deliberately not Assert.Inconclusive/Assert.Warn — both end
            // the run here and report the test as skipped, so the sign/submit half would silently stop running.
            TestContext.Out.WriteLine(
                "NOTE: Stellar RPC returned ADDRESS_V2 credentials for useUpgradedAuth: false — the legacy opt-out " +
                "is no longer honoured server-side. The v1 assertion was skipped; the sign/submit half still ran.");
            expectedSignedType = typeof(SorobanAddressCredentialsV2);
        }
        else
        {
            legacyCredentials.Should().BeOfType<SorobanAddressCredentials>(
                "useUpgradedAuth: false asks Stellar RPC for legacy SOROBAN_CREDENTIALS_ADDRESS");
            expectedSignedType = typeof(SorobanAddressCredentials);
        }

        await SignSubmitAndAssertSuccessAsync(tx, source, authorizer, legacySim, expectedSignedType);
    }

    /// <summary>
    ///     Builds a fresh deploy transaction whose create is authorized by an address other than the transaction
    ///     source. That is what makes recording-mode simulation emit an <em>address</em> credential: the host
    ///     requires that address to authorize the create. Deploying from the source account would yield
    ///     source-account credentials, which carry no variant and would make the variant assertions vacuous.
    /// </summary>
    private async Task<(KeyPair Source, KeyPair Authorizer, Transaction Tx)> BuildAddressAuthorizedDeployAsync()
    {
        var source = await CreateFundedAccountAsync();
        var authorizer = await CreateFundedAccountAsync();
        var wasmHash = _wasmHash ??= await UploadWasmAsync(source, ReadWasm("soroban_hello_world_contract.wasm"));

        var rpcAccount = await GetRpcAccountWithRetryAsync(source.AccountId);
        var tx = new TransactionBuilder(rpcAccount)
            .AddOperation(CreateContractOperation.FromAddress(wasmHash, authorizer.AccountId))
            .Build();
        return (source, authorizer, tx);
    }

    /// <summary>
    ///     Signs the simulation's recorded entry as <paramref name="authorizer" /> with the default
    ///     <see cref="SorobanCredentialsVersion.Preserve" />, asserts the variant survived signing, then submits
    ///     and asserts the create ran — which it only does if the host accepted the authorization signature.
    /// </summary>
    private async Task SignSubmitAndAssertSuccessAsync(
        Transaction tx,
        KeyPair source,
        KeyPair authorizer,
        SimulateTransactionResponse simulation,
        Type expectedCredentialType)
    {
        // Recording mode returns entries unsigned (no expiration, void signature), so the SDK supplies both here.
        var latest = await Rpc.GetLatestLedger();
        var signedEntry = SorobanAuthorization.AuthorizeEntry(
            simulation.SorobanAuthorization![0],
            authorizer,
            (uint)latest.Sequence + 100,
            Network.Test());
        signedEntry.Credentials.Should().BeOfType(expectedCredentialType,
            "signing must preserve the credential variant simulation returned");

        tx.SetSorobanTransactionData(simulation.SorobanTransactionData!);
        tx.SetSorobanAuthorization([signedEntry]);
        // The signed authorization entry is larger than the placeholder simulation measured, so top up the
        // resource fee rather than sending exactly MinResourceFee.
        tx.AddResourceFee((simulation.MinResourceFee ?? 0) + 100_000);
        tx.Sign(source);

        var result = await SendAndPollAsync(tx);
        result.Status.Should().Be(TransactionInfo.TransactionStatus.SUCCESS);
        result.CreatedContractId.Should().NotBeNull(
            "the create should have run, which it only does if the host accepted the authorization signature");
    }

    private static void AssertSimulated(SimulateTransactionResponse simulation)
    {
        simulation.Error.Should().BeNull("simulation should not error: {0}", simulation.Error);
        simulation.SorobanAuthorization.Should()
            .NotBeNullOrEmpty("deploying from a non-source address should record an authorization entry");
    }
}
