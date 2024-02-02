using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using stellar_dotnet_sdk.requests;
using stellar_dotnet_sdk.requests.sorobanrpc;
using stellar_dotnet_sdk.responses.sorobanrpc;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class SorobanServer : IDisposable
{
    private const string ClientNameHeader = "X-Client-Name";
    private const string ClientVersionHeader = "X-Client-Version";
    private readonly HttpClient _httpClient;
    private readonly bool _ownHttpClient;
    private readonly Uri _serverUri;

    public SorobanServer(string uri, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _serverUri = new Uri(uri);
        _ownHttpClient = false;
    }

    public SorobanServer(string uri)
        : this(uri, CreateHttpClient())
    {
        _ownHttpClient = true;
    }

    public void Dispose()
    {
        if (_ownHttpClient) _httpClient?.Dispose();
    }

    public static HttpClient CreateHttpClient()
    {
        return CreateHttpClient(new HttpClientHandler());
    }

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
    /// </summary>
    /// <returns>A GetHealthResponse object containing the health check result.</returns>
    public Task<GetHealthResponse> GetHealth()
    {
        return SendRequest<object, GetHealthResponse>("getHealth", null);
    }

    /// <summary>
    ///     Fetches metadata about the network which Soroban-RPC is connected to.
    /// </summary>
    /// <returns>A GetNetworkResponse object containing the network metadata.</returns>
    public Task<GetNetworkResponse> GetNetwork()
    {
        return SendRequest<object, GetNetworkResponse>("getNetwork", null);
    }

    /// <summary>
    ///     Fetch the details of a submitted transaction.
    ///     When submitting a transaction, client should poll this to tell when the transaction has
    ///     completed.
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
        return SendRequest<object, GetTransactionResponse>("getTransaction", new[] { txHash });
    }

    /// <summary>
    ///     Fetch a minimal set of current info about a Stellar account. Needed to get the current sequence
    ///     number for the account, so you can build a successful transaction with TransactionBuilder
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
        if (response.Entries == null || response.Entries.Length == 0)
            throw new AccountNotFoundException(accountId);
        var ledgerEntryAccount = LedgerEntryAccount.FromXdrBase64(response.Entries[0]!.Xdr);
        return new Account(ledgerEntryAccount.Account.AccountId, ledgerEntryAccount.SequenceNumber);
    }


    /// <summary>
    ///     Reads the current value of contract data ledger entries directly.
    ///     Allows you to directly inspect the current state of contracts, contract's code, or any other ledger entries.
    /// </summary>
    /// <param name="contractId">
    ///     contractId The contract ID containing the data to load. Encoded as Stellar Contract
    ///     Address. e.g. "CCJZ5DGASBWQXR5MPFCJXMBI333XE5U3FSJTNQU7RIKE3P5GN2K2WYD5"
    /// </param>
    /// <param name="key">The key of the contract data to load.</param>
    /// <param name="durability">
    ///     The "durability keyspace" that this ledger key belongs to, which is either TEMPORARY or
    ///     PERSISTENT.
    /// </param>
    /// <returns>A LedgerEntryResult object containing the ledger entry result.</returns>
    public async Task<GetLedgerEntriesResponse.LedgerEntryResult?> GetContractData(string contractId, SCVal key,
        ContractDataDurability durability)
    {
        SCAddress address = new SCContractId(contractId);
        var ledgerKeyContractData = new LedgerKeyContractData(address, key, durability);
        var response = await GetLedgerEntry(ledgerKeyContractData);
        return response.Entries.Length == 0 ? null : response.Entries[0];
    }

    public Task<GetLatestLedgerResponse> GetLatestLedger()
    {
        return SendRequest<object, GetLatestLedgerResponse>("getLatestLedger", null);
    }

    /// <summary>
    ///     Reads the current value of ledger entries directly.
    ///     Allows you to directly inspect the current state of contracts, contract's code, or any other ledger entries.
    /// </summary>
    /// <param name="keys">The key of the contract data to load, at least one key must be provided.</param>
    /// <returns>A GetLedgerEntriesResponse object containing the current values.</returns>
    public Task<GetLedgerEntriesResponse> GetLedgerEntries(LedgerKey[] keys)
    {
        var xdrBase64Keys = new string[keys.Length];
        for (var i = 0; i < keys.Length; i++) xdrBase64Keys[i] = keys[i].ToXdrBase64();

        return SendRequest<object, GetLedgerEntriesResponse>("getLedgerEntries", new { keys = xdrBase64Keys });
    }

    public Task<GetEventsResponse> GetEvents(GetEventsRequest request)
    {
        return SendRequest<object, GetEventsResponse>("getEvents", request);
    }

    public Task<GetLedgerEntriesResponse> GetLedgerEntry(LedgerKey key)
    {
        var xdrBase64Keys = new[] { key.ToXdrBase64() };

        return SendRequest<object, GetLedgerEntriesResponse>("getLedgerEntries", new { keys = xdrBase64Keys });
    }

    public Task<SimulateTransactionResponse> SimulateTransaction(Transaction tx, uint? resourceConfig = null)
    {
        if (resourceConfig != null)
            return SendRequest<object, SimulateTransactionResponse>("simulateTransaction",
                new
                {
                    transaction = tx.ToUnsignedEnvelopeXdrBase64(),
                    resourceConfig = new { instructionLeeway = resourceConfig }
                });

        return SendRequest<object, SimulateTransactionResponse>("simulateTransaction",
            new
            {
                transaction = tx.ToUnsignedEnvelopeXdrBase64()
            });
    }

    public Task<SendTransactionResponse> SendTransaction(Transaction tx)
    {
        return SendRequest<object, SendTransactionResponse>("sendTransaction",
            new { transaction = tx.ToEnvelopeXdrBase64() });
    }

    private async Task<TR> SendRequest<T, TR>(string method, T? parameters)
    {
        var responseHandler = new ResponseHandler<SorobanRpcResponse<TR>>();
        var requestId = GenerateRequestId();
        SorobanRpcRequest<T> sorobanRpcRequest = new(requestId, method, parameters);
        var httpContent = new StringContent(JsonConvert.SerializeObject(sorobanRpcRequest), Encoding.UTF8,
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