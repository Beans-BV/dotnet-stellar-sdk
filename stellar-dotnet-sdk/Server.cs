using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using stellar_dotnet_sdk.federation;
using stellar_dotnet_sdk.requests;
using stellar_dotnet_sdk.responses;

namespace stellar_dotnet_sdk;

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

    [Obsolete("Paths is deprecated in Horizon v1.0.0. Use PathStrictReceive.")]
    public PathsRequestBuilder Paths => new(_serverUri, _httpClient);

    public PathStrictSendRequestBuilder PathStrictSend => new(_serverUri, _httpClient);

    public PathStrictReceiveRequestBuilder PathStrictReceive => new(_serverUri, _httpClient);

    public IPaymentsRequestInitialBuilder Payments => PaymentsRequestBuilder.Create(_serverUri, _httpClient);

    public TransactionsRequestBuilder Transactions => new(_serverUri, _httpClient);

    public FriendBotRequestBuilder TestNetFriendBot => new(_serverUri, _httpClient);

    public TradesAggregationRequestBuilder TradeAggregations => new(_serverUri, _httpClient);
    public LiquidityPoolsRequestBuilder LiquidityPools => new(_serverUri, _httpClient);

    public void Dispose()
    {
        if (_internalHttpClient) _httpClient.Dispose();
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

    public Task<SubmitTransactionResponse?> SubmitTransaction(FeeBumpTransaction feeBump,
        SubmitTransactionOptions options)
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
        SubmitTransactionOptions options)
    {
        if (!options.SkipMemoRequiredCheck)
        {
            TransactionBase tx;

            if (options.FeeBumpTransaction)
                tx = FeeBumpTransaction.FromEnvelopeXdr(transactionEnvelopeBase64);
            else
                tx = Transaction.FromEnvelopeXdr(transactionEnvelopeBase64);

            await CheckMemoRequired(tx);
        }

        var transactionUriBuilder = new UriBuilder(_serverUri);

        var path = _serverUri.AbsolutePath.TrimEnd('/');
        transactionUriBuilder.SetPath($"{path}/transactions");

        var paramsPairs = new List<KeyValuePair<string, string>>
        {
            new("tx", transactionEnvelopeBase64)
        };

        var response =
            await _httpClient.PostAsync(transactionUriBuilder.Uri, new FormUrlEncodedContent(paramsPairs.ToArray()));
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (options.EnsureSuccess && !response.IsSuccessStatusCode)
        {
            throw new ConnectionErrorException(
                $"Status code ({response.StatusCode}) is not success.{(!string.IsNullOrEmpty(responseString) ? " Content: " + responseString : "")}");
        }

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
            case HttpStatusCode.BadRequest:
                var submitTransactionResponse = JsonSingleton.GetInstance<SubmitTransactionResponse>(
                    responseString);
                return submitTransactionResponse;
            case HttpStatusCode.ServiceUnavailable:
                throw new ServiceUnavailableException(
                    response.Headers.Contains("Retry-After")
                        ? response.Headers.GetValues("Retry-After").First()
                        : null
                );
            case HttpStatusCode.TooManyRequests:
                throw new TooManyRequestsException(
                    response.Headers.Contains("Retry-After")
                        ? response.Headers.GetValues("Retry-After").First()
                        : null
                );
            case HttpStatusCode.GatewayTimeout:
                throw new SubmitTransactionTimeoutResponseException();
            default:
                throw new SubmitTransactionUnknownResponseException(response.StatusCode, responseString);
        }
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

        if (tx.Memo != null && !Equals(tx.Memo, Memo.None())) return;

        var destinations = new HashSet<string>();

        foreach (var operation in tx.Operations)
        {
            if (!IsPaymentOperation(operation)) continue;

            // If it's a muxed account it already contains the memo.
            var destinationKey = PaymentOperationDestination(operation);
            if (destinationKey.IsMuxedAccount) continue;

            var destination = destinationKey.Address;

            if (!destinations.Add(destination)) continue;

            try
            {
                var account = await Accounts.Account(destination);
                if (!account.Data.TryGetValue(AccountRequiresMemoKey, out var value)) continue;

                if (value == AccountRequiresMemo)
                    throw new AccountRequiresMemoException("Account requires memo");
            }
            catch (HttpResponseException ex)
            {
                if (ex.StatusCode != 404)
                    throw;
            }
        }
    }

    public static HttpClient CreateHttpClient()
    {
        return CreateHttpClient(new HttpClientHandler());
    }

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
        return transaction switch
        {
            FeeBumpTransaction feeBump => feeBump.InnerTransaction,
            Transaction tx => tx,
            _ => throw new ArgumentException($"Invalid transaction of type {transaction.GetType().Name}")
        };
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
        return op switch
        {
            PaymentOperation p => p.Destination,
            PathPaymentStrictSendOperation p => p.Destination,
            PathPaymentStrictReceiveOperation p => p.Destination,
            AccountMergeOperation p => p.Destination,
            _ => throw new ArgumentException("Expected payment operation.")
        };
    }
}