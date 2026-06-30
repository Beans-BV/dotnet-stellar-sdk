using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
// Required: disambiguate these SDK types from the same-named ones in StellarDotnetSdk.Xdr (imported above).
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using Operation = StellarDotnetSdk.Operations.Operation;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk.IntegrationTests.Infrastructure;

/// <summary>
///     Base for Soroban integration tests. Owns a <see cref="StellarRpcServer" /> and provides the
///     simulate → assemble → sign → send → poll helpers plus a self-provisioning contract deploy
///     (upload + create the hello_world WASM), so tests never depend on a pre-existing contract id.
/// </summary>
public abstract class SorobanIntegrationTestBase : IntegrationTestBase
{
    protected StellarRpcServer Rpc = null!;

    [OneTimeSetUp]
    public void SorobanOneTimeSetUp()
    {
        Rpc = new StellarRpcServer(TestnetConfig.StellarRpcUrl, TestnetConfig.StellarRpcToken);
    }

    [OneTimeTearDown]
    public void SorobanOneTimeTearDown()
    {
        Rpc.Dispose();
    }

    /// <summary>Reads the hello_world WASM copied into the test output's TestData/Wasm folder.</summary>
    protected static byte[] HelloWorldWasm()
    {
        return File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "TestData", "Wasm", "soroban_hello_world_contract.wasm"));
    }

    /// <summary>Simulates the transaction, applies the returned Soroban data/auth + resource fee, and signs it.</summary>
    protected async Task SimulateAssembleSignAsync(Transaction tx, KeyPair signer)
    {
        var sim = await Rpc.SimulateTransaction(tx);
        sim.Error.Should().BeNull("simulation should not error: {0}", sim.Error);
        if (sim.SorobanTransactionData != null)
        {
            tx.SetSorobanTransactionData(sim.SorobanTransactionData);
        }
        if (sim.SorobanAuthorization != null)
        {
            tx.SetSorobanAuthorization(sim.SorobanAuthorization);
        }
        tx.AddResourceFee((sim.MinResourceFee ?? 0) + 100_000);
        tx.Sign(signer);
    }

    /// <summary>
    ///     Sends an assembled Soroban transaction and polls <c>GetTransaction</c> until SUCCESS/FAILED,
    ///     bounded by a 90s deadline. RPC lag/outage (never resolving) reports <see cref="Assert.Inconclusive(string)" />.
    /// </summary>
    protected async Task<GetTransactionResponse> SendAndPollAsync(Transaction tx)
    {
        var send = await Rpc.SendTransaction(tx);
        if (send.Status == SendTransactionResponse.SendTransactionStatus.ERROR)
        {
            Assert.Fail($"SendTransaction returned ERROR: {send.ErrorResultXdr}");
        }
        if (send.Status == SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER)
        {
            Assert.Inconclusive(
                $"SendTransaction returned TRY_AGAIN_LATER (RPC backpressure, not an SDK regression). Hash={send.Hash}");
        }
        // PENDING (normal) and DUPLICATE (already pending/included) both proceed to polling.

        var hash = send.Hash;
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            var get = await Rpc.GetTransaction(hash);
            switch (get.Status)
            {
                case TransactionInfo.TransactionStatus.SUCCESS:
                    return get;
                case TransactionInfo.TransactionStatus.FAILED:
                    Assert.Fail($"Soroban transaction {hash} FAILED: {get.ResultXdr}");
                    break;
                default:
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    break;
            }
        }

        Assert.Inconclusive(
            $"Soroban transaction {hash} did not resolve within 90s (RPC lag/outage, not an SDK regression).");
        return null!; // unreachable — Assert.Inconclusive throws
    }

    /// <summary>Builds a single-operation Soroban transaction, simulates/assembles/signs it, and sends+polls.</summary>
    protected async Task<GetTransactionResponse> RunSorobanAsync(KeyPair source, Operation operation)
    {
        var account = await Rpc.GetAccount(source.AccountId);
        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        await SimulateAssembleSignAsync(tx, source);
        return await SendAndPollAsync(tx);
    }

    /// <summary>Uploads the hello_world WASM and returns its hash (hex).</summary>
    protected async Task<string> UploadHelloWorldAsync(KeyPair source)
    {
        var result = await RunSorobanAsync(source, new UploadContractOperation(HelloWorldWasm()));
        result.WasmHash.Should().NotBeNull("upload should yield a WASM hash");
        return result.WasmHash!;
    }

    /// <summary>Creates a contract instance from a WASM hash and returns its contract id (StrKey C...).</summary>
    protected async Task<string> CreateContractAsync(KeyPair source, string wasmHash)
    {
        var result = await RunSorobanAsync(source, CreateContractOperation.FromAddress(wasmHash, source.AccountId));
        result.CreatedContractId.Should().NotBeNull("create should yield a contract id");
        return result.CreatedContractId!;
    }

    /// <summary>Uploads + creates the hello_world contract; returns the deployed contract id.</summary>
    protected async Task<string> DeployHelloWorldAsync(KeyPair source)
    {
        var wasmHash = await UploadHelloWorldAsync(source);
        return await CreateContractAsync(source, wasmHash);
    }

    /// <summary>Builds the ledger key for a contract's instance data entry (verbatim from SorobanHelpers).</summary>
    protected static LedgerKey CreateLedgerKeyContractData(string contractId)
    {
        var scContractId = new ScContractId(contractId);
        var contractDataDurability =
            ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT);
        return new LedgerKeyContractData(
            scContractId,
            new SCLedgerKeyContractInstance(),
            contractDataDurability);
    }
}