using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
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
///     Base for Soroban integration tests. Owns a <see cref="StellarRpcServer" /> (with a per-request
///     timeout) and provides the simulate → assemble → sign → send → poll helpers plus a
///     self-provisioning contract deploy (upload + create a WASM), so tests never depend on a
///     pre-existing contract id.
/// </summary>
public abstract class SorobanIntegrationTestBase : IntegrationTestBase
{
    private HttpClient _rpcHttp = null!;
    protected StellarRpcServer Rpc = null!;

    [OneTimeSetUp]
    public void SorobanOneTimeSetUp()
    {
        // Per-request timeout so a stalled RPC call fails fast instead of hanging on HttpClient's
        // ~100s default (the SDK sets none). This is what makes the poll deadlines below meaningful.
        _rpcHttp = new DefaultStellarSdkHttpClient(TestnetConfig.StellarRpcToken)
        {
            Timeout = TestnetConfig.HttpRequestTimeout,
        };
        Rpc = new StellarRpcServer(TestnetConfig.StellarRpcUrl, _rpcHttp);
    }

    [OneTimeTearDown]
    public void SorobanOneTimeTearDown()
    {
        Rpc.Dispose();
        _rpcHttp.Dispose();
    }

    /// <summary>Reads a WASM file copied into the test output's TestData/Wasm folder.</summary>
    protected static byte[] ReadWasm(string fileName)
    {
        return File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestData", "Wasm", fileName));
    }

    /// <summary>
    ///     Loads an account from the RPC, retrying briefly on <see cref="AccountNotFoundException" />
    ///     and on transient RPC errors. The account is funded via Friendbot (Horizon) but read here
    ///     from Stellar RPC — a different backend that ingests ledgers independently, so it can briefly
    ///     trail. A sustained miss is reported <see cref="Assert.Inconclusive(string)" />
    ///     (cross-backend lag or RPC outage, not an SDK regression).
    /// </summary>
    protected async Task<Account> GetRpcAccountWithRetryAsync(string accountId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        Exception? last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                return await Rpc.GetAccount(accountId);
            }
            catch (Exception ex) when (ex is AccountNotFoundException || IsTransientBackendError(ex))
            {
                last = ex;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        Assert.Inconclusive(
            $"Stellar RPC did not return account {accountId} within 30s of Horizon funding " +
            $"(cross-backend ingestion lag or RPC outage, not an SDK regression). {last?.Message}");
        return null!; // unreachable — Assert.Inconclusive throws
    }

    /// <summary>
    ///     Simulates the transaction, asserts it returned assembled Soroban data, applies that data +
    ///     authorization + resource fee, signs it, and returns the simulation response.
    /// </summary>
    protected async Task<SimulateTransactionResponse> SimulateAssembleSignAsync(Transaction tx, KeyPair signer)
    {
        var sim = await Rpc.SimulateTransaction(tx);
        sim.Error.Should().BeNull("simulation should not error: {0}", sim.Error);
        // A successful simulation must return assembled transaction data; otherwise the caller's
        // placeholder resources/fee would be signed and sent, failing on-chain as a confusing FAILED.
        sim.SorobanTransactionData.Should()
            .NotBeNull("a successful simulation should return assembled Soroban transaction data");
        tx.SetSorobanTransactionData(sim.SorobanTransactionData!);
        if (sim.SorobanAuthorization != null)
        {
            tx.SetSorobanAuthorization(sim.SorobanAuthorization);
        }
        tx.AddResourceFee((sim.MinResourceFee ?? 0) + 100_000);
        tx.Sign(signer);
        return sim;
    }

    /// <summary>
    ///     Sends an assembled Soroban transaction and polls <c>GetTransaction</c> until SUCCESS/FAILED,
    ///     bounded by a 90s deadline. A per-request timeout or transient network/RPC error (429/5xx) on
    ///     a poll is retried until the deadline; a sustained stall reports
    ///     <see cref="Assert.Inconclusive(string)" />.
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
            GetTransactionResponse get;
            try
            {
                get = await Rpc.GetTransaction(hash);
            }
            catch (Exception ex) when (IsTransientBackendError(ex))
            {
                // Per-request timeout / transient network or RPC error (429/5xx) — pause, then keep
                // polling until the deadline, which maps a sustained RPC stall to the Inconclusive
                // below rather than a hard failure. The pause also keeps a fast-failing endpoint from
                // being hammered in a tight loop.
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            switch (get.Status)
            {
                case TransactionInfo.TransactionStatus.SUCCESS:
                    return get;
                case TransactionInfo.TransactionStatus.FAILED:
                    throw new AssertionException($"Soroban transaction {hash} FAILED: {get.ResultXdr}");
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
        var account = await GetRpcAccountWithRetryAsync(source.AccountId);
        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        await SimulateAssembleSignAsync(tx, source);
        return await SendAndPollAsync(tx);
    }

    /// <summary>Uploads a contract WASM and returns its hash (hex).</summary>
    protected async Task<string> UploadWasmAsync(KeyPair source, byte[] wasm)
    {
        var result = await RunSorobanAsync(source, new UploadContractOperation(wasm));
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

    /// <summary>Uploads + creates a contract from its WASM bytes; returns the deployed contract id.</summary>
    protected async Task<string> DeployContractAsync(KeyPair source, byte[] wasm)
    {
        var wasmHash = await UploadWasmAsync(source, wasm);
        return await CreateContractAsync(source, wasmHash);
    }

    /// <summary>Uploads + creates the hello_world contract; returns the deployed contract id.</summary>
    protected Task<string> DeployHelloWorldAsync(KeyPair source)
    {
        return DeployContractAsync(source, ReadWasm("soroban_hello_world_contract.wasm"));
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