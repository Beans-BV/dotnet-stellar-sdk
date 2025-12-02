using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Federation;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk;

public class Server : IDisposable
{
    private const string ClientNameHeader = "X-Client-Name";
    private const string ClientVersionHeader = "X-Client-Version";
    private const string AccountRequiresMemo = "MQ=="; // "1" in base64. See SEP0029
    private const string AccountRequiresMemoKey = "config.memo_required";
    private readonly HttpClient _httpClient;
    private readonly bool _internalHttpClient;
    private readonly Uri _serverUri;

    public Server(string uri, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _serverUri = new Uri(uri);
        _internalHttpClient = false;
    }

    public Server(string uri)
        : this(uri, CreateHttpClient())
    {
        _internalHttpClient = true;
    }

    /// <summary>
    ///     Constructs a new instance that will interact with the provided URL.
    /// </summary>
    /// <param name="uri">URL of the Horizon server.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    public Server(string uri, string? bearerToken = null)
    {
        _serverUri = new Uri(uri);
        _httpClient = new DefaultStellarSdkHttpClient(bearerToken);
        _internalHttpClient = true;
    }

    /// <summary>
    ///     Constructs a new instance that will interact with the provided URL.
    /// </summary>
    /// <param name="uri">URL of the Horizon server.</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests. If null, default retry configuration is used.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    public Server(string uri, HttpResilienceOptions? resilienceOptions, string? bearerToken)
    {
        _serverUri = new Uri(uri);
        _httpClient = new DefaultStellarSdkHttpClient(bearerToken, resilienceOptions: resilienceOptions);
        _internalHttpClient = true;
    }

    public AccountsRequestBuilder Accounts => new(_serverUri, _httpClient);

    public AssetsRequestBuilder Assets => new(_serverUri, _httpClient);

    public ClaimableBalancesRequestBuilder ClaimableBalances => new(_serverUri, _httpClient);
    public EffectsRequestBuilder Effects => new(_serverUri, _httpClient);

    public LedgersRequestBuilder Ledgers => new(_serverUri, _httpClient);

    public OffersRequestBuilder Offers => new(_serverUri, _httpClient);

    public OperationsRequestBuilder Operations => new(_serverUri, _httpClient);

    public FeeStatsRequestBuilder FeeStats => new(_serverUri, _httpClient);

    public OrderBookRequestBuilder OrderBook => new(_serverUri, _httpClient);

    public TradesRequestBuilder Trades => new(_serverUri, _httpClient);

    public PathStrictSendRequestBuilder PathStrictSend => new(_serverUri, _httpClient);

    public PathStrictReceiveRequestBuilder PathStrictReceive => new(_serverUri, _httpClient);

    public IPaymentsRequestInitialBuilder Payments => PaymentsRequestBuilder.Create(_serverUri, _httpClient);

    public TransactionsRequestBuilder Transactions => new(_serverUri, _httpClient);

    public FriendBotRequestBuilder TestNetFriendBot => new(_serverUri, _httpClient);

    public TradesAggregationRequestBuilder TradeAggregations => new(_serverUri, _httpClient);
    public LiquidityPoolsRequestBuilder LiquidityPools => new(_serverUri, _httpClient);

    public void Dispose()
    {
        if (_internalHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    public RootResponse Root()
    {
        return RootAsync().Result;
    }

    public async Task<RootResponse> RootAsync()
    {
        var responseHandler = new ResponseHandler<RootResponse>();
        var response = await _httpClient.GetAsync(_serverUri);

        return await responseHandler.HandleResponse(response);
    }

    /// <summary>
    ///     Submit a transaction to the network.
    ///     This method will check if any of the destination accounts require a memo.
    /// </summary>
    /// <param name="transaction">A signed transaction object.</param>
    public Task<SubmitTransactionResponse?> SubmitTransaction(Transaction transaction)
    {
        var options = new SubmitTransactionOptions { SkipMemoRequiredCheck = false };
        return SubmitTransaction(transaction.ToEnvelopeXdrBase64(), options);
    }

    /// <summary>
    ///     Submit a transaction to the network.
    ///     This method will check if any of the destination accounts require a memo.  Change the SkipMemoRequiredCheck
    ///     options to change this behaviour.
    /// </summary>
    public Task<SubmitTransactionResponse?> SubmitTransaction(Transaction transaction, SubmitTransactionOptions options)
    {
        return SubmitTransaction(transaction.ToEnvelopeXdrBase64(), options);
    }

    /// <summary>
    ///     Submit a transaction to the network.
    ///     This method will check if any of the destination accounts require a memo.
    /// </summary>
    public Task<SubmitTransactionResponse?> SubmitTransaction(string transactionEnvelopeBase64)
    {
        var options = new SubmitTransactionOptions { SkipMemoRequiredCheck = false };
        return SubmitTransaction(transactionEnvelopeBase64, options);
    }

    public Task<SubmitTransactionResponse?> SubmitTransaction(FeeBumpTransaction feeBump)
    {
        var options = new SubmitTransactionOptions { FeeBumpTransaction = true };
        return SubmitTransaction(feeBump.ToEnvelopeXdrBase64(), options);
    }

    public Task<SubmitTransactionResponse?> SubmitTransaction(
        FeeBumpTransaction feeBump,
        SubmitTransactionOptions options
    )
    {
        options.FeeBumpTransaction = true;
        return SubmitTransaction(feeBump.ToEnvelopeXdrBase64(), options);
    }

    /// <summary>
    ///     Submit a transaction to the network.
    ///     This method will check if any of the destination accounts require a memo.  Change the SkipMemoRequiredCheck
    ///     options to change this behaviour.
    /// </summary>
    public async Task<SubmitTransactionResponse?> SubmitTransaction(
        string transactionEnvelopeBase64,
        SubmitTransactionOptions options
    )
    {
        return await SubmitTransaction<SubmitTransactionResponse>(
            transactionEnvelopeBase64,
            options,
            "transactions"
        );
    }

    /// <summary>
    ///     This endpoint submits transactions to the Stellar network asynchronously.
    ///     It is designed to allow users to submit transactions without blocking them while waiting for a response from
    ///     Horizon. At the same time, it also provides clear response status codes from stellar-core to help understand the
    ///     status of the submitted transaction.
    /// </summary>
    /// <param name="transactionEnvelopeBase64"></param>
    /// <returns></returns>
    /// <exception cref="ServiceUnavailableException"></exception>
    /// <exception cref="TooManyRequestsException"></exception>
    /// <exception cref="SubmitTransactionTimeoutResponseException"></exception>
    /// <exception cref="SubmitTransactionUnknownResponseException"></exception>
    public async Task<SubmitTransactionAsyncResponse?> SubmitTransactionAsync(
        string transactionEnvelopeBase64,
        SubmitTransactionOptions options
    )
    {
        return await SubmitTransaction<SubmitTransactionAsyncResponse>(
            transactionEnvelopeBase64,
            options,
            "transactions_async"
        );
    }

    private async Task<T?> SubmitTransaction<T>(
        string transactionEnvelopeBase64,
        SubmitTransactionOptions options,
        string endpoint
    ) where T : class
    {
        if (!options.SkipMemoRequiredCheck)
        {
            TransactionBase tx;

            if (options.FeeBumpTransaction)
            {
                tx = FeeBumpTransaction.FromEnvelopeXdr(transactionEnvelopeBase64);
            }
            else
            {
                tx = Transaction.FromEnvelopeXdr(transactionEnvelopeBase64);
            }
            await CheckMemoRequired(tx);
        }
        var transactionUriBuilder = new UriBuilder(_serverUri);

        var path = _serverUri.AbsolutePath.TrimEnd('/');
        transactionUriBuilder.SetPath($"{path}/{endpoint}");

        var paramsPairs = new List<KeyValuePair<string, string>>
        {
            new("tx", transactionEnvelopeBase64),
        };

        var response = await _httpClient.PostAsync(transactionUriBuilder.Uri, new FormUrlEncodedContent(paramsPairs));
        var responseString = await response.Content.ReadAsStringAsync();

        if (options.EnsureSuccess && !response.IsSuccessStatusCode)
        {
            throw new ConnectionErrorException(
                $"Status code ({response.StatusCode}) is not success.{(!string.IsNullOrEmpty(responseString) ? " Content: " + responseString : "")}");
        }
        return HandleResponse<T>(response, responseString);
    }

    /// <summary>
    ///     Submit a transaction asynchronously to the network.
    ///     This method will check if any of the destination accounts require a memo.
    /// </summary>
    /// <param name="transaction">A signed transaction object.</param>
    public Task<SubmitTransactionAsyncResponse?> SubmitTransactionAsync(Transaction transaction)
    {
        var options = new SubmitTransactionOptions { SkipMemoRequiredCheck = false };
        return SubmitTransactionAsync(transaction.ToEnvelopeXdrBase64(), options);
    }

    /// <summary>
    ///     Submit a transaction asynchronously to the network.
    ///     This method will check if any of the destination accounts require a memo.  Change the SkipMemoRequiredCheck
    ///     options to change this behaviour.
    /// </summary>
    public Task<SubmitTransactionAsyncResponse?> SubmitTransactionAsync(
        Transaction transaction,
        SubmitTransactionOptions options
    )
    {
        return SubmitTransactionAsync(transaction.ToEnvelopeXdrBase64(), options);
    }

    /// <summary>
    ///     Submit a transaction asynchronously to the network.
    ///     This method will check if any of the destination accounts require a memo.
    /// </summary>
    public Task<SubmitTransactionAsyncResponse?> SubmitTransactionAsync(string transactionEnvelopeBase64)
    {
        var options = new SubmitTransactionOptions { SkipMemoRequiredCheck = false };
        return SubmitTransactionAsync(transactionEnvelopeBase64, options);
    }

    public Task<SubmitTransactionAsyncResponse?> SubmitTransactionAsync(FeeBumpTransaction feeBump)
    {
        var options = new SubmitTransactionOptions { FeeBumpTransaction = true };
        return SubmitTransactionAsync(feeBump.ToEnvelopeXdrBase64(), options);
    }

    public Task<SubmitTransactionAsyncResponse?> SubmitTransactionAsync(
        FeeBumpTransaction feeBump,
        SubmitTransactionOptions options
    )
    {
        options.FeeBumpTransaction = true;
        return SubmitTransactionAsync(feeBump.ToEnvelopeXdrBase64(), options);
    }

    private T? HandleResponse<T>(HttpResponseMessage response, string responseString) where T : class
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
            case HttpStatusCode.Created:
            case HttpStatusCode.BadRequest:
                return JsonSerializer.Deserialize<T>(responseString, JsonOptions.DefaultOptions);
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.TooManyRequests:
                throw CreateExceptionWithRetryInfo(response);
            case HttpStatusCode.GatewayTimeout:
                throw new SubmitTransactionTimeoutResponseException();
            default:
                throw new SubmitTransactionUnknownResponseException(response.StatusCode, responseString);
        }
    }

    private static Exception CreateExceptionWithRetryInfo(HttpResponseMessage response)
    {
        const string retryAfterHeader = "Retry-After";
        var retryAfter = response.Headers.Contains(retryAfterHeader)
            ? response.Headers.GetValues(retryAfterHeader).First()
            : null;

        return response.StatusCode == HttpStatusCode.ServiceUnavailable
            ? new ServiceUnavailableException(retryAfter)
            : new TooManyRequestsException(retryAfter);
    }

    /// <summary>
    ///     Check whether any of the destination accounts require a memo.
    ///     This method implements the checks defined in
    ///     <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0029.md">SEP0029</a>.
    ///     It will sequentially load each destination account and check if it has the data field
    ///     <c>config.memo_required</c> set to <c>"MQ=="</c>.
    /// </summary>
    public async Task CheckMemoRequired(TransactionBase transaction)
    {
        var tx = GetTransactionToCheck(transaction);

        if (!Equals(tx.Memo, Memo.None()))
        {
            return;
        }

        var destinations = new HashSet<string>();

        foreach (var operation in tx.Operations)
        {
            if (!IsPaymentOperation(operation))
            {
                continue;
            }

            // If it's a muxed account it already contains the memo.
            var destinationKey = PaymentOperationDestination(operation);
            if (destinationKey.IsMuxedAccount)
            {
                continue;
            }

            var destination = destinationKey.Address;

            if (!destinations.Add(destination))
            {
                continue;
            }

            try
            {
                var account = await Accounts.Account(destination);
                if (!account.Data.TryGetValue(AccountRequiresMemoKey, out var value))
                {
                    continue;
                }

                if (value == AccountRequiresMemo)
                {
                    throw new AccountRequiresMemoException("Account requires memo");
                }
            }
            catch (HttpResponseException ex)
            {
                if (ex.StatusCode != 404)
                {
                    throw;
                }
            }
        }
    }

    [Obsolete(
        "Pass your own HttpClient instance to Server(string uri, HttpClient httpClient) instead. Otherwise call Server(string uri). Will be removed in the next major version.")]
    public static HttpClient CreateHttpClient()
    {
        return CreateHttpClient(new HttpClientHandler());
    }

    [Obsolete(
        "Pass your own HttpClient instance to Server(string uri, HttpClient httpClient) instead. Otherwise call Server(string uri). Will be removed in the next major version.")]
    public static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var assembly = Assembly.GetAssembly(typeof(Server))?.GetName();
        httpClient.DefaultRequestHeaders.Add(ClientNameHeader, assembly?.Name);
        httpClient.DefaultRequestHeaders.Add(ClientVersionHeader, assembly?.Version?.ToString());
        return httpClient;
    }

    private Transaction GetTransactionToCheck(TransactionBase transaction)
    {
        switch (transaction)
        {
            case FeeBumpTransaction feeBump:
                return feeBump.InnerTransaction;
            case Transaction tx:
                return tx;
            default:
                throw new ArgumentException($"Invalid transaction of type {transaction.GetType().Name}");
        }
    }

    private bool IsPaymentOperation(Operation op)
    {
        switch (op)
        {
            case PaymentOperation _:
            case PathPaymentStrictSendOperation _:
            case PathPaymentStrictReceiveOperation _:
            case AccountMergeOperation _:
                return true;
            default:
                return false;
        }
    }

    private IAccountId PaymentOperationDestination(Operation op)
    {
        switch (op)
        {
            case PaymentOperation p:
                return p.Destination;
            case PathPaymentStrictSendOperation p:
                return p.Destination;
            case PathPaymentStrictReceiveOperation p:
                return p.Destination;
            case AccountMergeOperation p:
                return p.Destination;
            default:
                throw new ArgumentException("Expected payment operation.");
        }
    }
}