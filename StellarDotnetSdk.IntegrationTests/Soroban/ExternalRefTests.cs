using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Soroban;

/// <summary>
///     Live coverage for Protocol 28's CAP-85 external executable references. Writing a tag entry needs an owner
///     contract that calls <c>create_executable_tag</c>, which no Wasm fixture in this suite does, so these tests pin
///     the parts a live network can prove without one: that Stellar RPC parses the SDK's <c>SCV_EXECUTABLE_TAG</c>
///     ledger key and reports the entry as missing, and that the host decodes a
///     <c>CONTRACT_EXECUTABLE_EXTERNAL_REF</c> deployment and rejects it for the right reason. A binary (non-UTF-8)
///     tag is used throughout, so the wire carries bytes that a text-only XDR string would reject.
///     <para>
///         These tests cannot tell whether the key holds the right tag bytes: with no tag written, a corrupted tag is
///         missing too. That the SDK builds the exact key and operation bytes is pinned instead by the known-answer
///         vectors in <c>Protocol28Test</c>.
///     </para>
/// </summary>
[TestFixture]
[CancelAfter(300_000)]
public class ExternalRefTests : SorobanIntegrationTestBase
{
    private static byte[] NewBinaryTag()
    {
        var tag = new byte[16];
        RandomNumberGenerator.Fill(tag);
        // 0xff never appears in UTF-8, so the tag is guaranteed not to be text.
        tag[0] = 0xff;
        return tag;
    }

    /// <summary>
    ///     Resolving a reference whose tag was never written reports the entry as missing (not archived). The owner's
    ///     instance entry is read first as a control, so the miss is about the tag key and not the owner.
    /// </summary>
    [Test]
    public async Task GetExternalRefWasmHash_TagNeverWritten_ThrowsExternalRefNotFoundException()
    {
        await RequireProtocol28Async();
        var account = await CreateFundedAccountAsync();
        var owner = await DeployHelloWorldAsync(account);
        await WaitForOwnerInstanceAsync(owner);
        var tag = NewBinaryTag();

        // The retry runs outside any FluentAssertions throw-assertion: one would catch the retry's
        // Assert.Inconclusive as an unexpected exception type and fail the test instead. Transient errors propagate
        // to the retry; any other exception is captured for the assertions below.
        var thrown = await WithTransientRetryAsync(async () =>
        {
            try
            {
                await Rpc.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(owner), tag));
                return null;
            }
            catch (Exception ex) when (!IsTransientBackendError(ex))
            {
                return ex;
            }
        }, "getLedgerEntries for the tag entry");

        var exception = thrown.Should().BeOfType<ExternalRefNotFoundException>(
            "a tag that was never written must be reported as missing, but the call ended with {0}", thrown).Subject;
        exception.IsArchived.Should().BeFalse();
        exception.OwnerContractId.Should().Be(owner);
        exception.Tag.Should().Equal(tag);
    }

    /// <summary>
    ///     A deployment from an external reference whose tag was never written is decoded by Stellar RPC and rejected
    ///     by the host during simulation, rather than failing to parse.
    /// </summary>
    [Test]
    public async Task SimulateTransaction_CreateContractFromUnwrittenExternalRef_IsRejectedByHost()
    {
        await RequireProtocol28Async();
        var account = await CreateFundedAccountAsync();
        var owner = await DeployHelloWorldAsync(account);
        await WaitForOwnerInstanceAsync(owner);
        var rpcAccount = await GetRpcAccountWithRetryAsync(account.AccountId);
        var tx = new TransactionBuilder(rpcAccount)
            .AddOperation(CreateContractOperation.FromExternalRef(owner, NewBinaryTag(), account.AccountId))
            .Build();

        var simulation = await WithTransientRetryAsync(() => Rpc.SimulateTransaction(tx), "simulateTransaction");

        // The host resolves the reference by reading the owner's tag entry, so a missing entry surfaces as a
        // storage error. A malformed envelope would instead fail to decode, before the host ever ran. The exact
        // diagnostic is asserted on purpose: any other failure (or a different missing entry) would let this test
        // pass for the wrong reason. If a host release rewords it, update the expected text here.
        simulation.Error.Should().Contain("Error(Storage, MissingValue)",
            "the host should decode the external reference and then fail to find its tag entry");
    }

    /// <summary>
    ///     Reports <see cref="Assert.Inconclusive(string)" /> when the network runs a protocol older than 28, which
    ///     has no <c>SCV_EXECUTABLE_TAG</c> or <c>CONTRACT_EXECUTABLE_EXTERNAL_REF</c> and would reject both for a
    ///     reason unrelated to the SDK.
    /// </summary>
    private async Task RequireProtocol28Async()
    {
        var protocolVersion = (await WithTransientRetryAsync(() => Rpc.GetLatestLedger(), "getLatestLedger"))
            .ProtocolVersion;
        if (protocolVersion < 28)
        {
            Assert.Inconclusive(
                $"The network runs protocol {protocolVersion}; CAP-85 external references need protocol 28.");
        }
    }

    /// <summary>
    ///     Waits until Stellar RPC serves the just-deployed owner's instance entry, so a later miss is about the tag
    ///     key and not about RPC still catching up with the deployment. A sustained miss is ingestion lag or an
    ///     outage, reported as <see cref="Assert.Inconclusive(string)" />.
    /// </summary>
    private async Task WaitForOwnerInstanceAsync(string owner)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        Exception? last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var control = await Rpc.GetLedgerEntries([CreateLedgerKeyContractData(owner)]);
                if (control.LedgerEntries is { Length: > 0 })
                {
                    return;
                }
            }
            catch (Exception ex) when (IsTransientBackendError(ex))
            {
                last = ex;
            }
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        Assert.Inconclusive(
            $"Stellar RPC did not serve the instance of the just-deployed contract {owner} within 30s " +
            $"(ingestion lag or RPC outage, not an SDK regression). {last?.Message}");
    }

    /// <summary>
    ///     Runs an RPC call, retrying it on transient backend errors (timeouts, connection failures, 429/5xx) for up
    ///     to 30 seconds and then reporting <see cref="Assert.Inconclusive(string)" />, per the suite's flakiness
    ///     policy. Every other exception, including the SDK exceptions under test, propagates unchanged.
    /// </summary>
    private static async Task<T> WithTransientRetryAsync<T>(Func<Task<T>> call, string what)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        Exception? last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                return await call();
            }
            catch (Exception ex) when (IsTransientBackendError(ex))
            {
                last = ex;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
        Assert.Inconclusive(
            $"{what} kept failing with transient backend errors for 30s (not an SDK regression). {last?.Message}");
        return default!; // unreachable — Assert.Inconclusive throws
    }
}
