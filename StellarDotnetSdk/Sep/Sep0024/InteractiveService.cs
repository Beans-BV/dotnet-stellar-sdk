using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Implements SEP-0024 v3.8.0 - Hosted Deposit and Withdrawal for Stellar anchors.
///     SEP-0024 defines a standard protocol for anchors to facilitate deposits and
///     withdrawals using an interactive web interface. This allows users to convert
///     between Stellar assets and fiat currencies or other external assets.
///     The interactive flow works as follows:
///     1. Client authenticates with SEP-10 WebAuth to get a JWT token
///     2. Client calls the deposit or withdraw endpoint with asset and amount
///     3. Server returns a URL for an interactive web interface
///     4. Client displays the URL in a popup/webview where user completes KYC
///     5. User provides additional details (bank account, amounts, etc.)
///     6. Server processes the transaction and updates status
///     7. Client polls the transaction status endpoint for updates
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0024.md">SEP-0024</a>
///     <para>
///         <strong>HttpClient Usage:</strong> For production code, it is strongly recommended to pass a shared
///         <see cref="HttpClient" /> instance to the constructor or <see cref="FromDomainAsync" />. If no HttpClient
///         is provided, a new instance will be created internally. While <see cref="InteractiveService" /> implements
///         <see cref="IDisposable" /> to clean up internal clients, reusing a single HttpClient instance
///         (or using <see cref="System.Net.Http.IHttpClientFactory" />) is more efficient and avoids socket exhaustion
///         under load.
///     </para>
///     <para>
///         <b>IDisposable and HttpClient ownership</b>
///     </para>
///     <para>
///         <see cref="InteractiveService" /> implements <see cref="IDisposable" /> because it may create and own an internal
///         <see cref="HttpClient" /> instance. When you let <see cref="InteractiveService" /> create its own client (for
///         example, by calling the constructor without passing an <see cref="HttpClient" />), you should either wrap
///         <see cref="InteractiveService" /> in a <c>using</c> block or explicitly call <see cref="Dispose" /> when you are
///         finished with it so the internal client is cleaned up.
///     </para>
///     <para>
///         If you pass an external <see cref="HttpClient" /> into the constructor or
///         <see cref="FromDomainAsync(string, HttpResilienceOptions?, string?, HttpClient?, Dictionary{string, string}?, CancellationToken)" />,
///         that client remains owned by the caller. In that case, disposing <see cref="InteractiveService" /> will not dispose
///         the external client, and you are responsible for managing the <see cref="HttpClient" /> lifecycle yourself
///         (for example, by reusing a single long-lived instance for performance and resilience).
///     </para>
///     <para>
///         <b>HttpResilienceOptions</b>
///     </para>
///     <para>
///         You can configure retry policies, timeouts, and circuit breaker behavior by passing
///         <see cref="HttpResilienceOptions" /> to the constructor or <see cref="FromDomainAsync" />. These options
///         are only used when creating an internal <see cref="HttpClient" />. If you provide your own
///         <see cref="HttpClient" />, the resilience options are ignored.
///     </para>
/// </summary>
public class InteractiveService : IDisposable
{
    private const string AuthenticationRequiredType = "authentication_required";

    private readonly string _transferServiceAddress;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly bool _internalHttpClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InteractiveService" /> class with explicit transfer server address.
    /// </summary>
    /// <param name="transferServiceAddress">The transfer server SEP-24 URL.</param>
    /// <param name="httpClient">
    ///     Optional HTTP client instance. <strong>Recommended:</strong> Pass a shared HttpClient instance
    ///     for production use to avoid creating a new client per instance. If null, a new HttpClient will be
    ///     created internally and disposed when this instance is disposed.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in requests.</param>
    /// <param name="resilienceOptions">
    ///     Optional resilience options for HTTP requests (retries, timeouts). Ignored if
    ///     httpClient is provided.
    /// </param>
    public InteractiveService(
        string transferServiceAddress,
        HttpClient? httpClient = null,
        HttpResilienceOptions? resilienceOptions = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        _transferServiceAddress = transferServiceAddress ?? throw new ArgumentNullException(nameof(transferServiceAddress));
        _httpRequestHeaders = httpRequestHeaders;
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _internalHttpClient = false;
        }
        else
        {
            _httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
            _internalHttpClient = true;
        }
    }

    /// <summary>
    ///     Creates an instance of this class by loading the transfer server SEP-24 URL from the given domain's stellar.toml file.
    /// </summary>
    /// <param name="domain">The domain hosting the stellar.toml file.</param>
    /// <param name="resilienceOptions">
    ///     Resilience options for HTTP requests (applies to stellar.toml fetch and all SEP-24
    ///     requests)
    /// </param>
    /// <param name="bearerToken">Optional bearer token for stellar.toml fetch only (not used for SEP-24 requests)</param>
    /// <param name="httpClient">
    ///     Optional HTTP client instance. <strong>Recommended:</strong> Pass a shared HttpClient instance
    ///     for production use to avoid creating a new client per instance. If null, a new HttpClient will be
    ///     created internally and disposed when the returned instance is disposed.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in requests.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An instance of InteractiveService configured with the transfer server URL.</returns>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml file cannot be loaded or parsed.</exception>
    /// <exception cref="ArgumentException">Thrown when TRANSFER_SERVER_SEP0024 is not available for the domain.</exception>
    public static async Task<InteractiveService> FromDomainAsync(
        string domain,
        HttpResilienceOptions? resilienceOptions = null,
        string? bearerToken = null,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var toml = await StellarToml.FromDomainAsync(domain, resilienceOptions, bearerToken, httpClient, httpRequestHeaders)
            .ConfigureAwait(false);
        var addr = toml.GeneralInformation.TransferServerSep24;
        if (string.IsNullOrWhiteSpace(addr))
        {
            throw new ArgumentException($"Transfer server SEP 24 not available for domain {domain}", nameof(domain));
        }

        return new InteractiveService(addr, httpClient, resilienceOptions, httpRequestHeaders);
    }

    /// <summary>
    ///     Gets the anchor's basic info about what their TRANSFER_SERVER_SEP0024 supports to wallets and clients.
    /// </summary>
    /// <param name="lang">Language code specified using ISO 639-1. Description fields in the response should be in this language. Defaults to en.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Anchor capabilities information.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<Sep24InfoResponse> InfoAsync(string? lang = null, CancellationToken cancellationToken = default)
    {
        var uri = AppendEndpointToUrl(_transferServiceAddress, "info");
        var queryParams = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(lang))
        {
            queryParams["lang"] = lang;
        }

        var requestUri = BuildUriWithQuery(uri, queryParams);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddCustomHeaders(request);

        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseString);
        }

        return JsonSerializer.Deserialize<Sep24InfoResponse>(responseString, JsonOptions.DefaultOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    ///     Gets the anchor's reported fee that would be charged for a given deposit or withdraw operation.
    ///     This is important to allow an anchor to accurately report fees to a user even when the fee schedule is complex.
    ///     If a fee can be fully expressed with the fee_fixed, fee_percent or fee_minimum fields in the /info response,
    ///     then an anchor will not implement this endpoint.
    /// </summary>
    /// <param name="request">The fee request containing operation type, asset code, and amount.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The calculated fee response.</returns>
    /// <exception cref="Sep24AuthenticationRequiredException">Thrown when the server responds with an authentication_required error.</exception>
    /// <exception cref="Sep24RequestException">Thrown when the server responds with an error and corresponding error message.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<Sep24FeeResponse> FeeAsync(Sep24FeeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var uri = AppendEndpointToUrl(_transferServiceAddress, "fee");
        var queryParams = new Dictionary<string, string>
        {
            { "operation", request.Operation },
            { "asset_code", request.AssetCode },
            { "amount", request.Amount.ToString("G") },
        };

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            queryParams["type"] = request.Type;
        }

        var requestUri = BuildUriWithQuery(uri, queryParams);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddCustomHeaders(httpRequest);

        if (!string.IsNullOrWhiteSpace(request.Jwt))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Jwt);
        }

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            HandleForbiddenResponse(responseString);
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseString);
        }

        return JsonSerializer.Deserialize<Sep24FeeResponse>(responseString, JsonOptions.DefaultOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    ///     A deposit is when a user sends an external token (BTC via Bitcoin, USD via bank transfer, etc...)
    ///     to an address held by an anchor. In turn, the anchor sends an equal amount of tokens on the
    ///     Stellar network (minus fees) to the user's Stellar account.
    ///     The deposit endpoint allows a wallet to get deposit information from an anchor, so a user has
    ///     all the information needed to initiate a deposit. It also lets the anchor specify additional
    ///     information that the user must submit interactively via a popup or embedded browser
    ///     window to be able to deposit.
    /// </summary>
    /// <param name="request">The deposit request containing asset code, account, and optional parameters.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Interactive response containing URL and transaction ID.</returns>
    /// <exception cref="Sep24AuthenticationRequiredException">Thrown when the server responds with an authentication_required error.</exception>
    /// <exception cref="Sep24RequestException">Thrown when the server responds with an error and corresponding error message.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<Sep24InteractiveResponse> DepositAsync(Sep24DepositRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var uri = AppendEndpointToUrl(_transferServiceAddress, "transactions/deposit/interactive");
        var (fields, files) = BuildFormData(request);

        var content = new MultipartFormDataContent();
        foreach (var field in fields)
        {
            content.Add(new StringContent(field.Value), field.Key);
        }

        foreach (var file in files)
        {
            content.Add(new ByteArrayContent(file.Value), file.Key, file.Key);
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = content,
        };
        AddCustomHeaders(httpRequest);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Jwt);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            HandleForbiddenResponse(responseString);
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseString);
        }

        return JsonSerializer.Deserialize<Sep24InteractiveResponse>(responseString, JsonOptions.DefaultOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    ///     This operation allows a user to redeem an asset currently on the Stellar network for the real asset (BTC, USD, stock, etc...) via the anchor of the Stellar asset.
    ///     The withdraw endpoint allows a wallet to get withdrawal information from an anchor, so a user has all the information needed to initiate a withdrawal.
    ///     It also lets the anchor specify the url for the interactive webapp to continue with the anchor's side of the withdraw.
    /// </summary>
    /// <param name="request">The withdrawal request containing asset code, account, and optional parameters.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Interactive response containing URL and transaction ID.</returns>
    /// <exception cref="Sep24AuthenticationRequiredException">Thrown when the server responds with an authentication_required error.</exception>
    /// <exception cref="Sep24RequestException">Thrown when the server responds with an error and corresponding error message.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<Sep24InteractiveResponse> WithdrawAsync(Sep24WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var uri = AppendEndpointToUrl(_transferServiceAddress, "transactions/withdraw/interactive");
        var (fields, files) = BuildFormData(request);

        var content = new MultipartFormDataContent();
        foreach (var field in fields)
        {
            content.Add(new StringContent(field.Value), field.Key);
        }

        foreach (var file in files)
        {
            content.Add(new ByteArrayContent(file.Value), file.Key, file.Key);
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = content,
        };
        AddCustomHeaders(httpRequest);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Jwt);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            HandleForbiddenResponse(responseString);
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseString);
        }

        return JsonSerializer.Deserialize<Sep24InteractiveResponse>(responseString, JsonOptions.DefaultOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    ///     The transaction history endpoint helps anchors enable a better experience for users using an external wallet.
    ///     With it, wallets can display the status of deposits and withdrawals while they process and a history of past transactions with the anchor.
    ///     It's only for transactions that are deposits to or withdrawals from the anchor.
    ///     It returns a list of transactions from the account encoded in the authenticated JWT.
    /// </summary>
    /// <param name="request">The transactions request containing asset code and optional filters.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>List of transactions matching the request criteria.</returns>
    /// <exception cref="Sep24AuthenticationRequiredException">Thrown when the server responds with an authentication_required error.</exception>
    /// <exception cref="Sep24RequestException">Thrown when the server responds with an error and corresponding error message.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<Sep24TransactionsResponse> TransactionsAsync(Sep24TransactionsRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var uri = AppendEndpointToUrl(_transferServiceAddress, "transactions");
        var queryParams = new Dictionary<string, string>
        {
            { "asset_code", request.AssetCode },
        };

        if (request.NoOlderThan.HasValue)
        {
            queryParams["no_older_than"] = request.NoOlderThan.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
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

        var requestUri = BuildUriWithQuery(uri, queryParams);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddCustomHeaders(httpRequest);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Jwt);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            HandleForbiddenResponse(responseString);
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseString);
        }

        return JsonSerializer.Deserialize<Sep24TransactionsResponse>(responseString, JsonOptions.DefaultOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    ///     The transaction endpoint enables clients to query/validate a specific transaction at an anchor.
    ///     Anchors must ensure that the SEP-10 JWT included in the request contains the Stellar account
    ///     and optional memo value used when making the original deposit or withdraw request
    ///     that resulted in the transaction requested using this endpoint.
    /// </summary>
    /// <param name="request">The transaction request containing transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Detailed transaction information.</returns>
    /// <exception cref="Sep24TransactionNotFoundException">Thrown when the server could not find the transaction.</exception>
    /// <exception cref="Sep24AuthenticationRequiredException">Thrown when the server responds with an authentication_required error.</exception>
    /// <exception cref="Sep24RequestException">Thrown when the server responds with an error and corresponding error message.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<Sep24TransactionResponse> TransactionAsync(Sep24TransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var uri = AppendEndpointToUrl(_transferServiceAddress, "transaction");
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

        var requestUri = BuildUriWithQuery(uri, queryParams);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddCustomHeaders(httpRequest);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Jwt);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new Sep24TransactionNotFoundException();
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            HandleForbiddenResponse(responseString);
        }

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseString);
        }

        return JsonSerializer.Deserialize<Sep24TransactionResponse>(responseString, JsonOptions.DefaultOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private static string AppendEndpointToUrl(string baseUrl, string endpoint)
    {
        var baseUri = baseUrl.TrimEnd('/');
        var endpointPath = endpoint.TrimStart('/');
        return $"{baseUri}/{endpointPath}";
    }

    private static string BuildUriWithQuery(string baseUri, Dictionary<string, string> queryParams)
    {
        if (queryParams.Count == 0)
        {
            return baseUri;
        }

        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{baseUri}?{queryString}";
    }

    private void AddCustomHeaders(HttpRequestMessage request)
    {
        if (_httpRequestHeaders == null)
        {
            return;
        }

        foreach (var header in _httpRequestHeaders)
        {
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static (Dictionary<string, string> Fields, Dictionary<string, byte[]> Files) BuildFormData(Sep24DepositRequest request)
    {
        var fields = new Dictionary<string, string>
        {
            { "asset_code", request.AssetCode },
        };

        if (!string.IsNullOrWhiteSpace(request.AssetIssuer))
        {
            fields["asset_issuer"] = request.AssetIssuer;
        }

        if (!string.IsNullOrWhiteSpace(request.SourceAsset))
        {
            fields["source_asset"] = request.SourceAsset;
        }

        if (!string.IsNullOrWhiteSpace(request.Amount))
        {
            fields["amount"] = request.Amount;
        }

        if (!string.IsNullOrWhiteSpace(request.QuoteId))
        {
            fields["quote_id"] = request.QuoteId;
        }

        if (!string.IsNullOrWhiteSpace(request.Account))
        {
            fields["account"] = request.Account;
        }

        if (!string.IsNullOrWhiteSpace(request.MemoType))
        {
            fields["memo_type"] = request.MemoType;
        }

        if (!string.IsNullOrWhiteSpace(request.Memo))
        {
            fields["memo"] = request.Memo;
        }

        if (!string.IsNullOrWhiteSpace(request.WalletName))
        {
            fields["wallet_name"] = request.WalletName;
        }

        if (!string.IsNullOrWhiteSpace(request.WalletUrl))
        {
            fields["wallet_url"] = request.WalletUrl;
        }

        if (!string.IsNullOrWhiteSpace(request.Lang))
        {
            fields["lang"] = request.Lang;
        }

        if (!string.IsNullOrWhiteSpace(request.ClaimableBalanceSupported))
        {
            fields["claimable_balance_supported"] = request.ClaimableBalanceSupported;
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            fields["customer_id"] = request.CustomerId;
        }

        var files = new Dictionary<string, byte[]>();

        // Add KYC fields
        if (request.KycFields != null)
        {
            if (request.KycFields.NaturalPerson != null)
            {
                foreach (var kvp in request.KycFields.NaturalPerson.GetFields())
                {
                    fields[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in request.KycFields.NaturalPerson.GetFiles())
                {
                    files[kvp.Key] = kvp.Value;
                }
            }

            if (request.KycFields.Organization != null)
            {
                foreach (var kvp in request.KycFields.Organization.GetFields())
                {
                    fields[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in request.KycFields.Organization.GetFiles())
                {
                    files[kvp.Key] = kvp.Value;
                }
            }
        }

        // Add custom fields and files
        if (request.CustomFields != null)
        {
            foreach (var kvp in request.CustomFields)
            {
                fields[kvp.Key] = kvp.Value;
            }
        }

        if (request.CustomFiles != null)
        {
            foreach (var kvp in request.CustomFiles)
            {
                files[kvp.Key] = kvp.Value;
            }
        }

        return (fields, files);
    }

    private static (Dictionary<string, string> Fields, Dictionary<string, byte[]> Files) BuildFormData(Sep24WithdrawRequest request)
    {
        var fields = new Dictionary<string, string>
        {
            { "asset_code", request.AssetCode },
        };

        if (!string.IsNullOrWhiteSpace(request.DestinationAsset))
        {
            fields["destination_asset"] = request.DestinationAsset;
        }

        if (!string.IsNullOrWhiteSpace(request.AssetIssuer))
        {
            fields["asset_issuer"] = request.AssetIssuer;
        }

        if (!string.IsNullOrWhiteSpace(request.Amount))
        {
            fields["amount"] = request.Amount;
        }

        if (!string.IsNullOrWhiteSpace(request.QuoteId))
        {
            fields["quote_id"] = request.QuoteId;
        }

        if (!string.IsNullOrWhiteSpace(request.Account))
        {
            fields["account"] = request.Account;
        }

        if (!string.IsNullOrWhiteSpace(request.Memo))
        {
            fields["memo"] = request.Memo;
        }

        if (!string.IsNullOrWhiteSpace(request.MemoType))
        {
            fields["memo_type"] = request.MemoType;
        }

        if (!string.IsNullOrWhiteSpace(request.WalletName))
        {
            fields["wallet_name"] = request.WalletName;
        }

        if (!string.IsNullOrWhiteSpace(request.WalletUrl))
        {
            fields["wallet_url"] = request.WalletUrl;
        }

        if (!string.IsNullOrWhiteSpace(request.Lang))
        {
            fields["lang"] = request.Lang;
        }

        if (!string.IsNullOrWhiteSpace(request.RefundMemo))
        {
            fields["refund_memo"] = request.RefundMemo;
        }

        if (!string.IsNullOrWhiteSpace(request.RefundMemoType))
        {
            fields["refund_memo_type"] = request.RefundMemoType;
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            fields["customer_id"] = request.CustomerId;
        }

        var files = new Dictionary<string, byte[]>();

        // Add KYC fields
        if (request.KycFields != null)
        {
            if (request.KycFields.NaturalPerson != null)
            {
                foreach (var kvp in request.KycFields.NaturalPerson.GetFields())
                {
                    fields[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in request.KycFields.NaturalPerson.GetFiles())
                {
                    files[kvp.Key] = kvp.Value;
                }
            }

            if (request.KycFields.Organization != null)
            {
                foreach (var kvp in request.KycFields.Organization.GetFields())
                {
                    fields[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in request.KycFields.Organization.GetFiles())
                {
                    files[kvp.Key] = kvp.Value;
                }
            }
        }

        // Add custom fields and files
        if (request.CustomFields != null)
        {
            foreach (var kvp in request.CustomFields)
            {
                fields[kvp.Key] = kvp.Value;
            }
        }

        if (request.CustomFiles != null)
        {
            foreach (var kvp in request.CustomFiles)
            {
                files[kvp.Key] = kvp.Value;
            }
        }

        return (fields, files);
    }

    private static void HandleForbiddenResponse(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("type", out var typeElement))
            {
                var type = typeElement.GetString();
                if (type == AuthenticationRequiredType)
                {
                    throw new Sep24AuthenticationRequiredException();
                }
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, fall through to generic error handling
        }
    }

    private static void HandleErrorResponse(HttpStatusCode statusCode, string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                var errorMessage = errorElement.GetString();
                throw new Sep24RequestException(errorMessage ?? "Unknown error");
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, throw generic exception
        }

        throw new HttpRequestException($"Request failed with status code {(int)statusCode}: {responseBody}");
    }

    /// <summary>
    ///     Disposes the internal HttpClient if it was created by this instance.
    /// </summary>
    public void Dispose()
    {
        if (_internalHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}

