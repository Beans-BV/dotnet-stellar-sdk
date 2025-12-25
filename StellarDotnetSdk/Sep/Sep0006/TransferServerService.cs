using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0006.Exceptions;
using StellarDotnetSdk.Sep.Sep0006.Requests;
using StellarDotnetSdk.Sep.Sep0006.Responses;

namespace StellarDotnetSdk.Sep.Sep0006;

/// <summary>
///     Implements SEP-0006 Programmatic Deposit and Withdrawal API.
///     This service implements SEP-0006, which defines a non-interactive
///     protocol for deposits and withdrawals between Stellar assets and external systems
///     (fiat, crypto, etc.). Unlike SEP-0024's interactive flow, SEP-0006 is designed
///     for programmatic integration where all required information can be provided in
///     API requests.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0006.md">SEP-0006</a>
/// </summary>
/// <remarks>
///     <para>
///         <b>HttpClient Usage:</b> It is strongly recommended to pass a shared <see cref="HttpClient" />
///         instance to this service. HttpClient is designed to be long-lived and reused across multiple
///         requests. Creating a new HttpClient for each request can exhaust available sockets and lead
///         to <see cref="System.Net.Sockets.SocketException" /> errors under load.
///     </para>
///     <para>
///         If no HttpClient is provided, an internal one will be created and disposed when this service
///         is disposed. For optimal performance and resource management, inject your application's
///         shared HttpClient instance (e.g., via IHttpClientFactory in ASP.NET Core applications).
///     </para>
/// </remarks>
public class TransferServerService : IDisposable
{
    private readonly string? _bearerToken;
    private readonly HttpClient? _httpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly HttpResilienceOptions? _resilienceOptions;
    private readonly string _transferServiceAddress;
    private bool _disposed;
    private HttpClient? _internalHttpClient;

    /// <summary>
    ///     Creates a TransferServerService instance with the specified transfer server address.
    ///     Use <see cref="FromDomainAsync" /> instead if you want to automatically discover the transfer
    ///     server URL from an anchor's stellar.toml file.
    /// </summary>
    /// <param name="transferServiceAddress">The base URL of the anchor's transfer server endpoint</param>
    /// <param name="httpClient">
    ///     A shared HttpClient instance for making requests. It is strongly recommended to provide
    ///     a shared instance to avoid socket exhaustion. If not provided, an internal client will be
    ///     created and disposed with this service.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom headers to include in all requests</param>
    public TransferServerService(
        string transferServiceAddress,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        if (string.IsNullOrWhiteSpace(transferServiceAddress))
        {
            throw new ArgumentException("Transfer service address cannot be null or empty",
                nameof(transferServiceAddress));
        }

        _transferServiceAddress = transferServiceAddress.TrimEnd('/');
        _httpClient = httpClient;
        _httpRequestHeaders = httpRequestHeaders;
    }

    /// <summary>
    ///     Creates a TransferServerService instance with resilience options and bearer token.
    /// </summary>
    /// <param name="transferServiceAddress">The base URL of the anchor's transfer server endpoint</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests</param>
    /// <param name="bearerToken">Bearer token in case the server requires it</param>
    /// <param name="httpRequestHeaders">Optional custom headers to include in all requests</param>
    public TransferServerService(
        string transferServiceAddress,
        HttpResilienceOptions? resilienceOptions,
        string? bearerToken = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        if (string.IsNullOrWhiteSpace(transferServiceAddress))
        {
            throw new ArgumentException("Transfer service address cannot be null or empty",
                nameof(transferServiceAddress));
        }

        _transferServiceAddress = transferServiceAddress.TrimEnd('/');
        _resilienceOptions = resilienceOptions;
        _bearerToken = bearerToken;
        _httpRequestHeaders = httpRequestHeaders;
    }

    /// <summary>
    ///     Creates a TransferServerService by automatically discovering the transfer
    ///     server URL from an anchor's stellar.toml file.
    ///     This is the recommended way to create a TransferServerService instance.
    /// </summary>
    /// <param name="domain">The anchor's domain name (e.g., 'testanchor.stellar.org')</param>
    /// <param name="httpClient">
    ///     A shared HttpClient instance for making requests. It is strongly recommended to provide
    ///     a shared instance to avoid socket exhaustion. If not provided, an internal client will be
    ///     created and disposed with this service.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom headers to include in all requests</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A configured TransferServerService instance ready to use</returns>
    /// <exception cref="TransferServerException">
    ///     Thrown when the stellar.toml file cannot be fetched or TRANSFER_SERVER is not
    ///     found
    /// </exception>
    public static async Task<TransferServerService> FromDomainAsync(
        string domain,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var toml = await StellarToml.FromDomainAsync(domain, httpClient, httpRequestHeaders, cancellationToken)
            .ConfigureAwait(false);
        var transferServer = toml.GeneralInformation.TransferServer;

        if (string.IsNullOrWhiteSpace(transferServer))
        {
            throw new TransferServerException($"Transfer server not found in stellar.toml of domain {domain}");
        }

        return new TransferServerService(transferServer, httpClient, httpRequestHeaders);
    }

    /// <summary>
    ///     Creates a TransferServerService by automatically discovering the transfer
    ///     server URL from an anchor's stellar.toml file with resilience options.
    /// </summary>
    /// <param name="domain">The anchor's domain name</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests</param>
    /// <param name="bearerToken">Bearer token in case the server requires it</param>
    /// <param name="httpClient">
    ///     A shared HttpClient instance for making requests. It is strongly recommended to provide
    ///     a shared instance to avoid socket exhaustion. If not provided, an internal client will be
    ///     created and disposed with this service.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom headers to include in all requests</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A configured TransferServerService instance ready to use</returns>
    /// <exception cref="TransferServerException">
    ///     Thrown when the stellar.toml file cannot be fetched or TRANSFER_SERVER is not
    ///     found
    /// </exception>
    public static async Task<TransferServerService> FromDomainAsync(
        string domain,
        HttpResilienceOptions? resilienceOptions,
        string? bearerToken = null,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var toml = await StellarToml
            .FromDomainAsync(domain, resilienceOptions, bearerToken, httpClient, httpRequestHeaders, cancellationToken)
            .ConfigureAwait(false);
        var transferServer = toml.GeneralInformation.TransferServer;

        if (string.IsNullOrWhiteSpace(transferServer))
        {
            throw new TransferServerException($"Transfer server not found in stellar.toml of domain {domain}");
        }

        return new TransferServerService(transferServer, resilienceOptions, bearerToken, httpRequestHeaders);
    }

    /// <summary>
    ///     Disposes the internal HttpClient if one was created.
    ///     Does not dispose externally provided HttpClient instances.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_internalHttpClient != null)
        {
            _internalHttpClient.Dispose();
            _internalHttpClient = null;
        }

        _disposed = true;
    }

    /// <summary>
    ///     Gets or creates an HttpClient instance.
    ///     Uses DefaultStellarSdkHttpClient without retries by default, matching the Server class pattern.
    /// </summary>
    private HttpClient GetOrCreateHttpClient()
    {
        if (_httpClient != null)
        {
            return _httpClient;
        }

        if (_internalHttpClient == null)
        {
            _internalHttpClient = new DefaultStellarSdkHttpClient(
                _bearerToken,
                resilienceOptions: _resilienceOptions);
        }

        return _internalHttpClient;
    }

    /// <summary>
    ///     Retrieves basic information about the anchor's transfer server capabilities.
    ///     Queries the /info endpoint to discover which assets the anchor supports for
    ///     deposit and withdrawal operations, along with required fields and fee structure
    ///     for each asset.
    /// </summary>
    /// <param name="language">Language code for error messages using RFC 4646 (defaults to 'en')</param>
    /// <param name="jwt">JWT token from SEP-10 authentication</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Information about supported assets and their requirements</returns>
    public async Task<InfoResponse> InfoAsync(string? language = null, string? jwt = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(language))
        {
            queryParams["lang"] = language;
        }

        return await ExecuteGetAsync<InfoResponse>("info", queryParams, jwt, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Initiates a deposit of an external asset to receive the equivalent Stellar asset.
    ///     A deposit occurs when a user sends an external asset (BTC, USD via bank transfer,
    ///     etc.) to an address held by an anchor. The anchor then sends an equivalent amount
    ///     of the Stellar asset (minus fees) to the user's Stellar account.
    /// </summary>
    /// <param name="request">Deposit request parameters</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Deposit instructions including how to send the external asset</returns>
    /// <exception cref="CustomerInformationNeededException">Thrown when additional KYC information is required</exception>
    /// <exception cref="CustomerInformationStatusException">Thrown when KYC status needs to be checked</exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when authentication is missing or invalid</exception>
    public async Task<DepositResponse> DepositAsync(DepositRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = BuildDepositQueryParams(request);
        return await ExecuteGetAsync<DepositResponse>("deposit", queryParams, request.Jwt, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Initiates a deposit with asset conversion between non-equivalent tokens.
    ///     Used when the anchor supports SEP-38 quotes and the user wants to deposit one
    ///     asset type and receive a different asset type on Stellar.
    /// </summary>
    /// <param name="request">Deposit exchange request with source asset, destination asset, and amount</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Deposit instructions for the cross-asset deposit</returns>
    /// <exception cref="CustomerInformationNeededException">Thrown when additional KYC information is required</exception>
    /// <exception cref="CustomerInformationStatusException">Thrown when KYC status needs to be checked</exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when authentication is missing or invalid</exception>
    public async Task<DepositResponse> DepositExchangeAsync(DepositExchangeRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = BuildDepositExchangeQueryParams(request);
        return await ExecuteGetAsync<DepositResponse>("deposit-exchange", queryParams, request.Jwt, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Initiates a withdrawal to redeem a Stellar asset for its off-chain equivalent.
    ///     A withdrawal occurs when a user redeems an asset on the Stellar network for its
    ///     equivalent off-chain asset via the anchor.
    /// </summary>
    /// <param name="request">Withdrawal request parameters</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Withdrawal instructions including the Stellar account to send funds to</returns>
    /// <exception cref="CustomerInformationNeededException">Thrown when additional KYC information is required</exception>
    /// <exception cref="CustomerInformationStatusException">Thrown when KYC status needs to be checked</exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when authentication is missing or invalid</exception>
    public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = BuildWithdrawQueryParams(request);
        return await ExecuteGetAsync<WithdrawResponse>("withdraw", queryParams, request.Jwt, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Initiates a withdrawal with asset conversion between non-equivalent tokens.
    ///     Used when the anchor supports SEP-38 quotes and the user wants to withdraw one
    ///     asset type from Stellar and receive a different asset type off-chain.
    /// </summary>
    /// <param name="request">Withdrawal exchange request with source asset, destination asset, and amount</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Withdrawal instructions for the cross-asset withdrawal</returns>
    /// <exception cref="CustomerInformationNeededException">Thrown when additional KYC information is required</exception>
    /// <exception cref="CustomerInformationStatusException">Thrown when KYC status needs to be checked</exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when authentication is missing or invalid</exception>
    public async Task<WithdrawResponse> WithdrawExchangeAsync(WithdrawExchangeRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = BuildWithdrawExchangeQueryParams(request);
        return await ExecuteGetAsync<WithdrawResponse>("withdraw-exchange", queryParams, request.Jwt, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves the fee structure for deposit or withdrawal operations.
    ///     This endpoint allows wallets to query the fee that would be charged for
    ///     a given deposit or withdrawal operation before initiating it.
    /// </summary>
    /// <param name="request">Fee request parameters</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A FeeResponse containing the calculated fee amount</returns>
    public async Task<FeeResponse> FeeAsync(FeeRequest request, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "operation", request.Operation },
            { "asset_code", request.AssetCode },
            { "amount", request.Amount.ToString(CultureInfo.InvariantCulture) },
        };

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            queryParams["type"] = request.Type;
        }

        return await ExecuteGetAsync<FeeResponse>("fee", queryParams, request.Jwt, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves transaction history for an account with the anchor.
    ///     Queries the /transactions endpoint to get the status of deposits and withdrawals
    ///     while they process, as well as a history of past transactions.
    /// </summary>
    /// <param name="request">Transaction history request with account, asset code, and optional filters</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>List of transactions with their current status and details</returns>
    public async Task<AnchorTransactionsResponse> TransactionsAsync(AnchorTransactionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "asset_code", request.AssetCode },
            { "account", request.Account },
        };

        if (request.NoOlderThan.HasValue)
        {
            queryParams["no_older_than"] = request.NoOlderThan.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        if (request.Limit.HasValue)
        {
            queryParams["limit"] = request.Limit.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(request.Kind))
        {
            queryParams["kind"] = request.Kind;
        }

        if (!string.IsNullOrWhiteSpace(request.PagingId))
        {
            queryParams["paging_id"] = request.PagingId;
        }

        if (!string.IsNullOrWhiteSpace(request.Lang))
        {
            queryParams["lang"] = request.Lang;
        }

        return await ExecuteGetAsync<AnchorTransactionsResponse>(
            "transactions",
            queryParams,
            request.Jwt,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Retrieves details for a specific transaction at the anchor.
    ///     Queries the /transaction endpoint to get the current status and details of
    ///     a specific deposit or withdrawal transaction.
    /// </summary>
    /// <param name="request">Transaction query request with at least one identifier</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Current status and details of the requested transaction</returns>
    public async Task<AnchorTransactionResponse> TransactionAsync(AnchorTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Id))
        {
            queryParams["id"] = request.Id;
        }

        if (!string.IsNullOrWhiteSpace(request.StellarTransactionId))
        {
            queryParams["stellar_transaction_id"] = request.StellarTransactionId;
        }

        if (!string.IsNullOrWhiteSpace(request.ExternalTransactionId))
        {
            queryParams["external_transaction_id"] = request.ExternalTransactionId;
        }

        if (!string.IsNullOrWhiteSpace(request.Lang))
        {
            queryParams["lang"] = request.Lang;
        }

        return await ExecuteGetAsync<AnchorTransactionResponse>("transaction", queryParams, request.Jwt,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Updates transaction information with additional fields requested by the anchor.
    ///     This endpoint allows clients to update a transaction with additional information
    ///     that the anchor has requested.
    /// </summary>
    /// <param name="request">Patch transaction request with transaction ID and fields to update</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The updated transaction details</returns>
    /// <exception cref="ArgumentException">Thrown when request.Fields is null</exception>
    /// <exception cref="CustomerInformationNeededException">Thrown when additional KYC information is required</exception>
    /// <exception cref="CustomerInformationStatusException">Thrown when KYC status needs to be checked</exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when authentication is missing or invalid</exception>
    public async Task<AnchorTransactionResponse> PatchTransactionAsync(PatchTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Fields == null)
        {
            throw new ArgumentException("Fields cannot be null", nameof(request));
        }

        var uri = BuildUri($"transactions/{request.Id}");
        var client = GetOrCreateHttpClient();
        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, uri);

        AddHeaders(httpRequest, request.Jwt);

        // SEP-6 spec requires fields to be wrapped in a "transaction" object
        var payload = new Dictionary<string, object>
        {
            { "transaction", request.Fields },
        };
        var jsonContent = JsonSerializer.Serialize(payload, JsonOptions.DefaultOptions);
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        // Handle 403 Forbidden responses specially to parse error types
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            HandleForbiddenResponse(errorContent);
        }

        var responseHandler = new ResponseHandler<AnchorTransactionResponse>();
        return await responseHandler.HandleResponse(response).ConfigureAwait(false);
    }

    private Dictionary<string, string> BuildDepositQueryParams(DepositRequest request)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "asset_code", request.AssetCode },
            { "account", request.Account },
        };

        AddIfNotNull(queryParams, "funding_method", request.FundingMethod);
        AddIfNotNull(queryParams, "memo_type", request.MemoType);
        AddIfNotNull(queryParams, "memo", request.Memo);
        AddIfNotNull(queryParams, "email_address", request.EmailAddress);
        AddIfNotNull(queryParams, "type", request.Type);
        AddIfNotNull(queryParams, "wallet_name", request.WalletName);
        AddIfNotNull(queryParams, "wallet_url", request.WalletUrl);
        AddIfNotNull(queryParams, "lang", request.Lang);
        AddIfNotNull(queryParams, "on_change_callback", request.OnChangeCallback);
        AddIfNotNull(queryParams, "amount", request.Amount);
        AddIfNotNull(queryParams, "country_code", request.CountryCode);
        AddIfNotNull(queryParams, "claimable_balance_supported", request.ClaimableBalanceSupported);
        AddIfNotNull(queryParams, "customer_id", request.CustomerId);
        AddIfNotNull(queryParams, "location_id", request.LocationId);

        if (request.ExtraFields != null)
        {
            foreach (var field in request.ExtraFields)
            {
                queryParams[field.Key] = field.Value;
            }
        }

        return queryParams;
    }

    private Dictionary<string, string> BuildDepositExchangeQueryParams(DepositExchangeRequest request)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "destination_asset", request.DestinationAsset },
            { "source_asset", request.SourceAsset },
            { "amount", request.Amount.ToString(CultureInfo.InvariantCulture) },
            { "account", request.Account },
        };

        AddIfNotNull(queryParams, "funding_method", request.FundingMethod);
        AddIfNotNull(queryParams, "quote_id", request.QuoteId);
        AddIfNotNull(queryParams, "memo_type", request.MemoType);
        AddIfNotNull(queryParams, "memo", request.Memo);
        AddIfNotNull(queryParams, "email_address", request.EmailAddress);
        AddIfNotNull(queryParams, "type", request.Type);
        AddIfNotNull(queryParams, "wallet_name", request.WalletName);
        AddIfNotNull(queryParams, "wallet_url", request.WalletUrl);
        AddIfNotNull(queryParams, "lang", request.Lang);
        AddIfNotNull(queryParams, "on_change_callback", request.OnChangeCallback);
        AddIfNotNull(queryParams, "country_code", request.CountryCode);
        AddIfNotNull(queryParams, "claimable_balance_supported", request.ClaimableBalanceSupported);
        AddIfNotNull(queryParams, "customer_id", request.CustomerId);
        AddIfNotNull(queryParams, "location_id", request.LocationId);

        if (request.ExtraFields != null)
        {
            foreach (var field in request.ExtraFields)
            {
                queryParams[field.Key] = field.Value;
            }
        }

        return queryParams;
    }

    private Dictionary<string, string> BuildWithdrawQueryParams(WithdrawRequest request)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "asset_code", request.AssetCode },
        };

        AddIfNotNull(queryParams, "funding_method", request.FundingMethod);
        AddIfNotNull(queryParams, "type", request.Type);
        AddIfNotNull(queryParams, "dest", request.Dest);
        AddIfNotNull(queryParams, "dest_extra", request.DestExtra);
        AddIfNotNull(queryParams, "account", request.Account);
        AddIfNotNull(queryParams, "memo", request.Memo);
        AddIfNotNull(queryParams, "memo_type", request.MemoType);
        AddIfNotNull(queryParams, "wallet_name", request.WalletName);
        AddIfNotNull(queryParams, "wallet_url", request.WalletUrl);
        AddIfNotNull(queryParams, "lang", request.Lang);
        AddIfNotNull(queryParams, "on_change_callback", request.OnChangeCallback);
        AddIfNotNull(queryParams, "amount", request.Amount);
        AddIfNotNull(queryParams, "country_code", request.CountryCode);
        AddIfNotNull(queryParams, "refund_memo", request.RefundMemo);
        AddIfNotNull(queryParams, "refund_memo_type", request.RefundMemoType);
        AddIfNotNull(queryParams, "customer_id", request.CustomerId);
        AddIfNotNull(queryParams, "location_id", request.LocationId);

        if (request.ExtraFields != null)
        {
            foreach (var field in request.ExtraFields)
            {
                queryParams[field.Key] = field.Value;
            }
        }

        return queryParams;
    }

    private Dictionary<string, string> BuildWithdrawExchangeQueryParams(WithdrawExchangeRequest request)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "source_asset", request.SourceAsset },
            { "destination_asset", request.DestinationAsset },
            { "amount", request.Amount.ToString(CultureInfo.InvariantCulture) },
        };

        AddIfNotNull(queryParams, "funding_method", request.FundingMethod);
        AddIfNotNull(queryParams, "type", request.Type);
        AddIfNotNull(queryParams, "dest", request.Dest);
        AddIfNotNull(queryParams, "dest_extra", request.DestExtra);
        AddIfNotNull(queryParams, "quote_id", request.QuoteId);
        AddIfNotNull(queryParams, "account", request.Account);
        AddIfNotNull(queryParams, "memo", request.Memo);
        AddIfNotNull(queryParams, "memo_type", request.MemoType);
        AddIfNotNull(queryParams, "wallet_name", request.WalletName);
        AddIfNotNull(queryParams, "wallet_url", request.WalletUrl);
        AddIfNotNull(queryParams, "lang", request.Lang);
        AddIfNotNull(queryParams, "on_change_callback", request.OnChangeCallback);
        AddIfNotNull(queryParams, "country_code", request.CountryCode);
        AddIfNotNull(queryParams, "refund_memo", request.RefundMemo);
        AddIfNotNull(queryParams, "refund_memo_type", request.RefundMemoType);
        AddIfNotNull(queryParams, "customer_id", request.CustomerId);
        AddIfNotNull(queryParams, "location_id", request.LocationId);

        if (request.ExtraFields != null)
        {
            foreach (var field in request.ExtraFields)
            {
                queryParams[field.Key] = field.Value;
            }
        }

        return queryParams;
    }

    private static void AddIfNotNull(Dictionary<string, string> dict, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        dict[key] = value;
    }

    private static void AddIfNotNull(Dictionary<string, string> dict, string key, decimal? value)
    {
        if (!value.HasValue)
        {
            return;
        }
        dict[key] = value.Value.ToString(CultureInfo.InvariantCulture);
    }

    private Uri BuildUri(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        var uriBuilder = new UriBuilder($"{_transferServiceAddress}/{endpoint}");

        if (queryParams != null && queryParams.Count > 0)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var param in queryParams)
            {
                query[param.Key] = param.Value;
            }

            uriBuilder.Query = query.ToString();
        }

        return uriBuilder.Uri;
    }

    private void AddHeaders(HttpRequestMessage request, string? jwt)
    {
        if (_httpRequestHeaders != null)
        {
            foreach (var header in _httpRequestHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(jwt))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }
    }

    private async Task<T> ExecuteGetAsync<T>(
        string endpoint,
        Dictionary<string, string>? queryParams = null,
        string? jwt = null,
        CancellationToken cancellationToken = default) where T : Response
    {
        var uri = BuildUri(endpoint, queryParams);
        var client = GetOrCreateHttpClient();
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);

        AddHeaders(httpRequest, jwt);

        var response = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        // Handle 403 Forbidden responses specially to parse error types
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            HandleForbiddenResponse(errorContent);
        }

        var responseHandler = new ResponseHandler<T>();
        return await responseHandler.HandleResponse(response).ConfigureAwait(false);
    }

    private static void HandleForbiddenResponse(string errorJson)
    {
        if (string.IsNullOrWhiteSpace(errorJson))
        {
            return;
        }

        // Try to parse the error type from the JSON response.
        // If parsing fails or the type is unknown, return and let ResponseHandler
        // handle it as a generic HTTP 403 error.
        string? type;
        try
        {
            using var document = JsonDocument.Parse(errorJson);
            if (!document.RootElement.TryGetProperty("type", out var typeProperty))
            {
                return;
            }
            type = typeProperty.GetString();
        }
        catch (JsonException)
        {
            // Malformed JSON - fall back to generic error handling
            return;
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            return;
        }

        // Handle SEP-6 specific error types as defined in:
        // https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0006.md
        switch (type)
        {
            case "non_interactive_customer_info_needed":
            {
                var response =
                    JsonSerializer.Deserialize<CustomerInformationNeededResponse>(errorJson,
                        JsonOptions.DefaultOptions);
                if (response != null)
                {
                    throw new CustomerInformationNeededException(response);
                }
                break;
            }
            case "customer_info_status":
            {
                var response =
                    JsonSerializer.Deserialize<CustomerInformationStatusResponse>(errorJson,
                        JsonOptions.DefaultOptions);
                if (response != null)
                {
                    throw new CustomerInformationStatusException(response);
                }
                break;
            }
            case "authentication_required":
            {
                throw new AuthenticationRequiredException();
            }
        }

        // Unknown type - fall back to generic error handling in ResponseHandler
    }
}