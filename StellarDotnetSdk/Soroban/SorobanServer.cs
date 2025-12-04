using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     This class helps you to connect to a local or remote Soroban RPC server
///     and send requests to the server. It parses the results and provides
///     corresponding response objects.
/// </summary>
public class SorobanServer : IDisposable
{
    private const string ClientNameHeader = "X-Client-Name";
    private const string ClientVersionHeader = "X-Client-Version";
    private readonly HttpClient _httpClient;
    private readonly bool _internalHttpClient;
    private readonly Uri _serverUri;

    /// <summary>
    ///     Constructs a new instance that will interact with the provided URL.
    /// </summary>
    /// <param name="uri">URL of the Soroban RPC server.</param>
    /// <param name="httpClient">HttpClient instance to use for requests.</param>
    public SorobanServer(string uri, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _serverUri = new Uri(uri);
        _internalHttpClient = false;
    }

    /// <summary>
    ///     Constructs a new instance that will interact with the provided URL.
    /// </summary>
    /// <param name="uri">URL of the Soroban RPC server.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    public SorobanServer(string uri, string? bearerToken = null)
    {
        _serverUri = new Uri(uri);
        _httpClient = new DefaultStellarSdkHttpClient(bearerToken);
        _internalHttpClient = true;
    }

    /// <summary>
    ///     Constructs a new instance that will interact with the provided URL.
    /// </summary>
    /// <param name="uri">URL of the Soroban RPC server.</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests. If null, default retry configuration is used.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    public SorobanServer(string uri, HttpResilienceOptions? resilienceOptions, string? bearerToken = null)
    {
        _serverUri = new Uri(uri);
        _httpClient = new DefaultStellarSdkHttpClient(bearerToken, resilienceOptions: resilienceOptions);
        _internalHttpClient = true;
    }

    public void Dispose()
    {
        if (_internalHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    [Obsolete(
        "Pass your own HttpClient instance to SorobanServer(string uri, HttpClient httpClient) instead. Otherwise call SorobanServer(string uri). Will be removed in the next major version.")]
    public static HttpClient CreateHttpClient()
    {
        return CreateHttpClient(new HttpClientHandler());
    }

    [Obsolete(
        "Pass your own HttpClient instance to SorobanServer(string uri, HttpClient httpClient) instead. Otherwise call SorobanServer(string uri). Will be removed in the next major version.")]
    public static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var assembly = Assembly.GetAssembly(typeof(SorobanServer)).GetName();
        httpClient.DefaultRequestHeaders.Add(ClientNameHeader, assembly.Name);
        httpClient.DefaultRequestHeaders.Add(ClientVersionHeader, assembly.Version.ToString());
        return httpClient;
    }

    /// <summary>
    ///     General node health check.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getHealth
    /// </summary>
    /// <returns>The <see cref="GetHealthResponse" /> object containing the health check result.</returns>
    public Task<GetHealthResponse> GetHealth()
    {
        return SendRequest<object, GetHealthResponse>("getHealth", null);
    }

    /// <summary>
    ///     General information about the currently configured network. This response will contain all the information needed
    ///     to successfully submit transactions to the network this node serves.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getNetwork
    /// </summary>
    /// <returns>The <see cref="GetNetworkResponse" /> object containing the network metadata.</returns>
    public Task<GetNetworkResponse> GetNetwork()
    {
        return SendRequest<object, GetNetworkResponse>("getNetwork", null);
    }

    /// <summary>
    ///     Fetch the details of a submitted transaction.
    ///     When submitting a transaction, client should poll this to tell when the transaction has
    ///     completed.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getTransaction
    /// </summary>
    /// <param name="hash">
    ///     The hash of the transaction to check. Encoded as a hex string.
    /// </param>
    /// <returns>
    ///     A GetTransactionResponse object containing the transaction status, result, and
    ///     other details.
    /// </returns>
    public Task<GetTransactionResponse> GetTransaction(string txHash)
    {
        return SendRequest<object, GetTransactionResponse>("getTransaction", new { hash = txHash });
    }

    /// <summary>
    ///     The <c>getTransactions</c> method return a detailed list of transactions starting from the user specified starting
    ///     point that you can paginate as long as the pages fall within the history retention of their corresponding RPC
    ///     provider.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getTransactions
    /// </summary>
    /// <param name="txHash"></param>
    /// <returns></returns>
    public Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request)
    {
        return SendRequest<object, GetTransactionsResponse>("getTransactions", request);
    }

    /// <summary>
    ///     Version information about the RPC and Captive core. RPC manages its own, pared-down version of Stellar Core
    ///     optimized for its own subset of needs. we'll refer to this as a "Captive Core" instance.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getVersionInfo
    /// </summary>
    /// <returns></returns>
    public Task<GetVersionInfoResponse> GetVersionInfo()
    {
        return SendRequest<object, GetVersionInfoResponse>("getVersionInfo", null);
    }

    /// <summary>
    ///     Statistics for charged inclusion fees. The inclusion fee statistics are calculated from the inclusion fees that
    ///     were paid for the transactions to be included onto the ledger. For Soroban transactions and Stellar transactions,
    ///     they each have their own inclusion fees and own surge pricing. Inclusion fees are used to prevent spam and
    ///     prioritize transactions during network traffic surge.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getFeeStats
    /// </summary>
    /// <returns></returns>
    public Task<GetFeeStatsResponse> GetFeeStats()
    {
        return SendRequest<object, GetFeeStatsResponse>("getFeeStats", null);
    }

    /// <summary>
    ///     Fetch a minimal set of current info about a Stellar account. Needed to get the current sequence
    ///     number for the account, so you can build a successful transaction with TransactionBuilder.
    /// </summary>
    /// <param name="accountId">
    ///     The public address of the account to load.
    /// </param>
    /// <returns>
    ///     An Account object containing the sequence number and current state of the account.
    /// </returns>
    public async Task<Account> GetAccount(string accountId)
    {
        var ledgerKeyAccount = new LedgerKeyAccount(accountId);
        var response = await GetLedgerEntry(ledgerKeyAccount);
        if (response.LedgerEntries?.Length == 0 ||
            response.LedgerEntries?[0] is not LedgerEntryAccount ledgerEntryAccount)
        {
            throw new AccountNotFoundException(accountId);
        }
        return new Account(ledgerEntryAccount.Account.AccountId, ledgerEntryAccount.SequenceNumber);
    }

    /// <summary>
    ///     For finding out the current latest known ledger of this node. This is a subset of the ledger info from Horizon.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getLatestLedger
    /// </summary>
    /// <returns></returns>
    public Task<GetLatestLedgerResponse> GetLatestLedger()
    {
        return SendRequest<object, GetLatestLedgerResponse>("getLatestLedger", null);
    }

    /// <summary>
    ///     Reads the current value of ledger entries directly.
    ///     Allows you to directly inspect the current state of contracts, contract's code, or any other ledger entries.
    ///     <para>
    ///         This is a backup way to access your contract data which may
    ///         not be available via events or simulateTransaction.
    ///         To fetch contract wasm byte-code, use the ContractCode ledger entry key.
    ///     </para>
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getLedgerEntries
    /// </summary>
    /// <param name="keys">
    ///     Array of <see cref="LedgerKey" /> containing the keys of the ledger entries you wish to retrieve, at
    ///     least one key must be provided.
    /// </param>
    /// <returns>A <see cref="GetLedgerEntriesResponse" /> object containing the current values.</returns>
    public Task<GetLedgerEntriesResponse> GetLedgerEntries(LedgerKey[] keys)
    {
        return SendRequest<object, GetLedgerEntriesResponse>("getLedgerEntries",
            new { keys = keys.Select(x => x.ToXdrBase64()).ToArray() });
    }

    /// <summary>
    ///     Get a filtered list of events emitted by a given ledger range.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/getEvents
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<GetEventsResponse> GetEvents(GetEventsRequest request)
    {
        return SendRequest<object, GetEventsResponse>("getEvents", request);
    }

    public Task<GetLedgerEntriesResponse> GetLedgerEntry(LedgerKey key)
    {
        var xdrBase64Keys = new[] { key.ToXdrBase64() };

        return SendRequest<object, GetLedgerEntriesResponse>("getLedgerEntries", new { keys = xdrBase64Keys });
    }

    /// <summary>
    ///     Submit a trial contract invocation to simulate how it would be executed by the network. This endpoint calculates
    ///     the effective transaction data, required authorizations, and minimal resource fee. It provides a way to test and
    ///     analyze the potential outcomes of a transaction without actually submitting it to the network.
    ///     See: https://developers.stellar.org/docs/data/rpc/api-reference/methods/simulateTransaction
    /// </summary>
    /// <param name="transaction">
    ///     The transaction object to be simulated.
    ///     <remarks>
    ///         In order for the RPC server to successfully simulate a Stellar transaction, the provided transaction must
    ///         contain only a single operation of the type <c>invokeHostFunction</c>.
    ///     </remarks>
    /// </param>
    /// <param name="resourceConfig">Contains configuration for how resources will be calculated when simulating transactions.</param>
    /// <param name="authMode">
    ///     Explicitly allows users to opt in to non-root authorization in recording mode.
    ///     <p>
    ///         Leaving this field unset will default to <see cref="AuthMode.ENFORCE" /> if auth entries are present,
    ///         <see cref="AuthMode.RECORD" /> otherwise.
    ///     </p>
    /// </param>
    /// <returns>A <see cref="SimulateTransactionResponse" /> object.</returns>
    public Task<SimulateTransactionResponse> SimulateTransaction(
        Transaction transaction,
        uint? resourceConfig = null,
        AuthMode? authMode = null)
    {
        var requestParams = BuildSimulateTransactionRequest(transaction, resourceConfig, authMode);

        return SendRequest<object, SimulateTransactionResponse>(
            "simulateTransaction",
            requestParams
        );
    }

    private static Dictionary<string, object> BuildSimulateTransactionRequest(
        Transaction transaction,
        uint? resourceConfig,
        AuthMode? authMode)
    {
        var request = new Dictionary<string, object>
        {
            ["transaction"] = transaction.ToUnsignedEnvelopeXdrBase64(),
        };

        if (resourceConfig != null)
        {
            request["resourceConfig"] = new { instructionLeeway = resourceConfig };
        }

        if (authMode != null)
        {
            request["authMode"] = authMode;
        }

        return request;
    }

    /// <summary>
    ///     Submit a real transaction to the stellar network. This is the only way to make changes “on-chain”. Unlike Horizon,
    ///     this does not wait for transaction completion. It simply validates and enqueues the transaction. Clients should
    ///     call getTransactionStatus to learn about transaction success/failure. This supports all transactions, not only
    ///     smart contract-related transactions.
    ///     See https://developers.stellar.org/docs/data/rpc/api-reference/methods/sendTransaction.
    /// </summary>
    /// <param name="transaction">The transaction object to be submitted.</param>
    /// <returns>A <see cref="SendTransactionResponse" /> response.</returns>
    public Task<SendTransactionResponse> SendTransaction(Transaction transaction)
    {
        return SendRequest<object, SendTransactionResponse>("sendTransaction",
            new { transaction = transaction.ToEnvelopeXdrBase64() });
    }

    /// <summary>
    ///     Submit a FeeBump transaction to the Stellar network. This is the only way to make changes “on-chain”.
    ///     Unlike Horizon, this does not wait for transaction completion. It simply validates and enqueues the transaction.
    ///     Clients should call getTransactionStatus to learn about transaction success/failure.
    ///     This supports all transactions, not only smart contract-related transactions.
    ///     See https://developers.stellar.org/docs/data/rpc/api-reference/methods/sendTransaction.
    /// </summary>
    /// <param name="feeBumpTransaction">The FeeBumpTransaction object to be submitted.</param>
    /// <returns>A <see cref="T:StellarDotnetSdk.Responses.SorobanRpc.SendTransactionResponse" /> response.</returns>
    public Task<SendTransactionResponse> SendTransaction(FeeBumpTransaction feeBumpTransaction)
    {
        return SendRequest<object, SendTransactionResponse>("sendTransaction",
            new { transaction = feeBumpTransaction.ToEnvelopeXdrBase64() });
    }

    private async Task<TR> SendRequest<T, TR>(string method, T? parameters)
    {
        var responseHandler = new ResponseHandler<SorobanRpcResponse<TR>>();
        var requestId = GenerateRequestId();
        SorobanRpcRequest<T> sorobanRpcRequest = new(requestId, method, parameters);
        var httpContent = new StringContent(JsonSerializer.Serialize(sorobanRpcRequest), Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(_serverUri, httpContent);
        var sorobanRpcResponse = await responseHandler.HandleResponse(response);
        return sorobanRpcResponse.Result;
    }

    private static string GenerateRequestId()
    {
        return Guid.NewGuid().ToString();
    }
}