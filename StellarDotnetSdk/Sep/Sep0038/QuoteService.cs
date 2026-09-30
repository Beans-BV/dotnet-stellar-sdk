using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly.Timeout;
using StellarDotnetSdk.Compatibility;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0001.Exceptions;
using StellarDotnetSdk.Sep.Sep0038.Exceptions;
using StellarDotnetSdk.Sep.Sep0038.Requests;
using StellarDotnetSdk.Sep.Sep0038.Responses;

namespace StellarDotnetSdk.Sep.Sep0038;

/// <summary>
///     Client for SEP-0038 v2.5.0 — Anchor RFQ API. Lets a wallet discover which assets an anchor exchanges, fetch
///     indicative prices, and request firm quotes whose id it then passes as <c>quote_id</c> to a SEP-6, SEP-24 or
///     SEP-31 transaction.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0038.md">SEP-0038</a>.
///     <para>
///         <b>Authentication</b>
///     </para>
///     <para>
///         <see cref="InfoAsync" />, <see cref="PricesAsync" /> and <see cref="PriceAsync" /> accept an optional
///         JWT, which the anchor may use to personalize the response. <see cref="PostQuoteAsync" /> and
///         <see cref="GetQuoteAsync" /> require one. Obtain it with SEP-10
///         (<see cref="Sep0010.ClientWebAuth.JwtTokenAsync" />) for <c>G...</c>/<c>M...</c> accounts or SEP-45
///         (<see cref="Sep0045.ClientWebAuthContract.JwtTokenAsync" />) for contract accounts.
///     </para>
///     <para>
///         <b>Amounts</b>
///     </para>
///     <para>
///         Amounts and prices are <see cref="decimal" /> values. Requests send them as invariant-culture strings,
///         keeping the scale given; responses are read exactly, and a value <see cref="decimal" /> cannot hold without
///         rounding fails with <see cref="UnexpectedResponseException" /> rather than being approximated.
///     </para>
///     <para>
///         <b>Errors</b>
///     </para>
///     <para>
///         Invalid arguments — including the sell/buy amount exclusivity rules — throw
///         <see cref="ArgumentException" /> from the returned task, before anything is sent. Error responses map to
///         <see cref="BadRequestException" /> (400), <see cref="PermissionDeniedException" /> (403),
///         <see cref="NotFoundException" /> (404) and <see cref="UnexpectedResponseException" /> (anything else, an
///         oversized body, or an unusable success body); all derive from <see cref="QuoteServerException" />.
///         Response bodies are read up to 1 MiB, and parsed with the SDK's hardened JSON options, which reject
///         duplicated mapped properties.
///     </para>
///     <para>
///         Transport failures surface as <see cref="HttpRequestException" /> (including a connection that drops
///         while a success body is being read) or <see cref="TaskCanceledException" /> (cancellation, or a timeout, in
///         which case its inner exception is a <see cref="TimeoutException" />). The whole exchange — headers and
///         body — is bounded by the <see cref="HttpClient.Timeout" /> of the client in use (and, for the internal
///         client, by <see cref="HttpResilienceOptions.RequestTimeout" />), so a server that trickles its body cannot
///         stall the caller. An internal client with <see cref="HttpResilienceOptions.EnableCircuitBreaker" /> set
///         throws Polly's <c>BrokenCircuitException</c> while the circuit is open, and a call on a disposed service
///         that owns its client throws <see cref="ObjectDisposedException" />.
///     </para>
///     <para>
///         <b>Retries</b>
///     </para>
///     <para>
///         <c>POST /quote</c> is not idempotent: every call creates a new firm quote that the anchor holds in reserve
///         until it expires. Do not pass <see cref="HttpResilienceOptionsPresets.ForHorizon" /> or
///         <see cref="HttpResilienceOptionsPresets.ForSoroban" />: both retry <c>POST</c> on a 408, 429 or 5xx
///         answer, so one call can reserve two quotes. Use <see cref="HttpResilienceOptionsPresets.WithConnectionRetries" /> (which
///         still replays a <c>POST</c> whose response was lost in transit) or
///         <see cref="HttpResilienceOptionsPresets.NoRetry" />.
///     </para>
///     <para>
///         <b>HttpClient ownership</b>
///     </para>
///     <para>
///         Pass a shared <see cref="HttpClient" /> (or one from <c>IHttpClientFactory</c>)
///         in production; it stays owned by the caller and is not disposed with this service. When none is passed,
///         the service creates its own, configured by <see cref="HttpResilienceOptions" />, and disposes it in
///         <see cref="Dispose" />.
///     </para>
/// </summary>
public class QuoteService : IDisposable
{
    /// <summary>
    ///     Upper bound on a response body. <c>GET /info</c> and <c>GET /prices</c> grow with the number of assets an
    ///     anchor lists, so this is larger than SEP-45's 512 KiB, but still stops a hostile or malfunctioning server
    ///     from exhausting memory by streaming an unbounded body.
    /// </summary>
    private const int MaxResponseBodyBytes = 1024 * 1024;

    /// <summary>Longest run of an anchor's <c>error</c> text copied into an exception message.</summary>
    private const int MaxEchoedErrorLength = 256;

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly bool _internalHttpClient;
    private readonly TimeSpan? _requestTimeout;
    private readonly string _serviceAddress;

    /// <summary>
    ///     Creates a client for the quote server at <paramref name="serviceAddress" />.
    /// </summary>
    /// <param name="serviceAddress">The anchor's <c>ANCHOR_QUOTE_SERVER</c> URL.</param>
    /// <param name="httpClient">
    ///     Optional HTTP client to use. It remains owned by the caller. When null, the service creates one and
    ///     disposes it in <see cref="Dispose" />.
    /// </param>
    /// <param name="resilienceOptions">
    ///     Optional retry and timeout options for the client the service creates. Ignored when
    ///     <paramref name="httpClient" /> is provided.
    /// </param>
    /// <param name="httpRequestHeaders">Optional headers added to every request.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="serviceAddress" /> is empty, is not an absolute URL, carries a query,
    ///     fragment or user information, or uses plain <c>http</c> for a host other than a loopback address (the JWT travels with every
    ///     authenticated request, so it must not go in cleartext; loopback is allowed for local development).
    /// </exception>
    public QuoteService(
        string serviceAddress,
        HttpClient? httpClient = null,
        HttpResilienceOptions? resilienceOptions = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        var problem = ValidateAddress(serviceAddress, allowLoopbackHttp: true);
        if (problem != null)
        {
            throw new ArgumentException(problem, nameof(serviceAddress));
        }

        // The address as parsed, not as given: Uri drops surrounding whitespace that would otherwise reach every
        // request URI and fail there.
        _serviceAddress = new Uri(serviceAddress, UriKind.Absolute).AbsoluteUri.TrimEnd('/');
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
            _requestTimeout = resilienceOptions?.RequestTimeout;
        }
    }

    /// <summary>
    ///     Creates a client for the quote server declared as <c>ANCHOR_QUOTE_SERVER</c> in the stellar.toml of
    ///     <paramref name="domain" />.
    /// </summary>
    /// <param name="domain">The anchor's home domain, for example <c>testanchor.stellar.org</c>.</param>
    /// <param name="resilienceOptions">
    ///     Optional retry and timeout options for the stellar.toml fetch and, when <paramref name="httpClient" /> is
    ///     null, for the client the service creates.
    /// </param>
    /// <param name="bearerToken">Optional bearer token for the stellar.toml fetch only.</param>
    /// <param name="httpClient">Optional HTTP client to use; it remains owned by the caller.</param>
    /// <param name="httpRequestHeaders">Optional headers added to the stellar.toml fetch and every request.</param>
    /// <param name="cancellationToken">Token to cancel the stellar.toml fetch.</param>
    /// <returns>A client for the anchor's quote server.</returns>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml cannot be fetched or parsed.</exception>
    /// <exception cref="NoAnchorQuoteServerFoundException">
    ///     Thrown when the stellar.toml does not declare <c>ANCHOR_QUOTE_SERVER</c>, or declares a value that is not
    ///     an absolute https URL without a query or fragment (plain http is refused here even for loopback).
    /// </exception>
    public static async Task<QuoteService> FromDomainAsync(
        string domain,
        HttpResilienceOptions? resilienceOptions = null,
        string? bearerToken = null,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var toml = await StellarToml.FromDomainAsync(domain, resilienceOptions, bearerToken, httpClient,
                httpRequestHeaders, cancellationToken)
            .ConfigureAwait(false);
        var address = toml.GeneralInformation.AnchorQuoteServer;
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new NoAnchorQuoteServerFoundException(domain);
        }

        // The address comes from the anchor, not from the caller, so a rejection is reported as an unusable
        // declaration rather than as an argument exception naming a parameter the caller never passed. Loopback
        // http is a local-development allowance for addresses a developer passes in directly. An anchor-declared
        // address must be https: plain http would put the JWT in cleartext on the network, and the loopback
        // allowance would let a hostile stellar.toml aim the JWT at a plain-http service on the user's machine.
        var problem = ValidateAddress(address!, allowLoopbackHttp: false);
        if (problem != null)
        {
            throw new NoAnchorQuoteServerFoundException(domain, address!, problem);
        }

        return new QuoteService(address!, httpClient, resilienceOptions, httpRequestHeaders);
    }

    /// <summary>
    ///     Checks a quote server address, returning why it is unusable or <see langword="null" /> when it is fine.
    /// </summary>
    private static string? ValidateAddress(string? address, bool allowLoopbackHttp)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "The quote server address must not be null or empty.";
        }

        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return "The quote server address must be an absolute http or https URL.";
        }

        // Before .NET 10, Uri.IsLoopback matches the name "localhost" case-sensitively; match LOCALHOST here too.
        var loopback = uri.IsLoopback || string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);
        if (uri.Scheme == Uri.UriSchemeHttp && !(allowLoopbackHttp && loopback))
        {
            return allowLoopbackHttp
                ? "The quote server address must use https (the JWT is sent with every authenticated request); " +
                  "plain http is allowed only for loopback addresses used in local development."
                : "The quote server address must use https (the JWT is sent with every authenticated request).";
        }

        // Endpoint paths are appended to the address, so a query or fragment would swallow them.
        if (uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            return "The quote server address must not contain a query or fragment.";
        }

        if (uri.UserInfo.Length > 0)
        {
            return "The quote server address must not contain user information.";
        }

        return null;
    }

    /// <summary>
    ///     <c>GET /info</c>: the Stellar and off-chain assets available for trading, with their delivery methods and
    ///     country codes.
    /// </summary>
    /// <param name="jwt">Optional SEP-10 or SEP-45 JWT; the anchor may use it to personalize the response.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The supported assets.</returns>
    /// <exception cref="QuoteServerException">
    ///     Thrown (as one of its subtypes) when the anchor answers with an error or an unusable response.
    /// </exception>
    /// <exception cref="HttpRequestException">Thrown when the request cannot be sent or the response not read.</exception>
    /// <exception cref="TaskCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is canceled, or when the timeout elapses (the inner
    ///     exception is then a <see cref="TimeoutException" />).
    /// </exception>
    public async Task<InfoResponse> InfoAsync(string? jwt = null, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, BuildUri("info", null), jwt);
        return await SendAsync<InfoResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     <c>GET /prices</c>: indicative prices of the assets available in exchange for the asset given in the
    ///     request.
    /// </summary>
    /// <param name="request">
    ///     Either the sell side (<see cref="PricesRequest.SellAsset" /> and <see cref="PricesRequest.SellAmount" />)
    ///     or the buy side (<see cref="PricesRequest.BuyAsset" /> and <see cref="PricesRequest.BuyAmount" />).
    /// </param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>
    ///     The prices: <see cref="PricesResponse.BuyAssets" /> for a sell-side request,
    ///     <see cref="PricesResponse.SellAssets" /> for a buy-side one.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the request mixes or omits the sell and buy sides.</exception>
    /// <exception cref="QuoteServerException">
    ///     Thrown (as one of its subtypes) when the anchor answers with an error or an unusable response.
    /// </exception>
    /// <exception cref="HttpRequestException">Thrown when the request cannot be sent or the response not read.</exception>
    /// <exception cref="TaskCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is canceled, or when the timeout elapses (the inner
    ///     exception is then a <see cref="TimeoutException" />).
    /// </exception>
    public async Task<PricesResponse> PricesAsync(PricesRequest request,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(request, nameof(request));
        var httpRequest = CreateRequest(HttpMethod.Get, BuildUri("prices", request.ToQueryParameters()), request.Jwt);
        // The spec pairs each request side with one response array; a body without it (an empty object, or the
        // other side's array) would otherwise surface as a null list the caller was told to expect populated.
        var sellSide = !string.IsNullOrWhiteSpace(request.SellAsset);
        return await SendAsync<PricesResponse>(httpRequest, cancellationToken, response =>
                (sellSide ? response.BuyAssets : response.SellAssets) == null
                    ? $"The quote server answered a {(sellSide ? "sell" : "buy")}-side GET /prices without " +
                      $"'{(sellSide ? "buy_assets" : "sell_assets")}'."
                    : null)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     <c>GET /price</c>: the indicative price for one asset pair, with its fee.
    /// </summary>
    /// <param name="request">The pair, exactly one of the two amounts, and the context (SEP-6 or SEP-31).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The indicative price.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when an asset is missing, when not exactly one amount is given, or when the context is SEP-24.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the context is not a defined value.</exception>
    /// <exception cref="QuoteServerException">
    ///     Thrown (as one of its subtypes) when the anchor answers with an error or an unusable response.
    /// </exception>
    /// <exception cref="HttpRequestException">Thrown when the request cannot be sent or the response not read.</exception>
    /// <exception cref="TaskCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is canceled, or when the timeout elapses (the inner
    ///     exception is then a <see cref="TimeoutException" />).
    /// </exception>
    public async Task<PriceResponse> PriceAsync(PriceRequest request, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(request, nameof(request));
        var httpRequest = CreateRequest(HttpMethod.Get, BuildUri("price", request.ToQueryParameters()), request.Jwt);
        return await SendAsync<PriceResponse>(httpRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     <c>POST /quote</c>: requests a firm quote. The anchor holds the quoted amount in reserve until the quote
    ///     expires. Requires a JWT.
    /// </summary>
    /// <param name="request">
    ///     The pair, exactly one of the two amounts, at most one delivery method, the context, and the JWT.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The firm quote; pass its <see cref="QuoteResponse.Id" /> as <c>quote_id</c> to use it.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when an asset or the JWT is missing, when not exactly one amount is given, or when both delivery
    ///     methods are given.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the context is not a defined value.</exception>
    /// <exception cref="QuoteServerException">
    ///     Thrown (as one of its subtypes) when the anchor answers with an error or an unusable response.
    /// </exception>
    /// <exception cref="HttpRequestException">Thrown when the request cannot be sent or the response not read.</exception>
    /// <exception cref="TaskCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is canceled, or when the timeout elapses (the inner
    ///     exception is then a <see cref="TimeoutException" />).
    /// </exception>
    public async Task<QuoteResponse> PostQuoteAsync(QuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(request, nameof(request));
        var body = request.ToJson();
        var httpRequest = CreateRequest(HttpMethod.Post, BuildUri("quote", null), request.Jwt,
            new StringContent(body, Encoding.UTF8, "application/json"));
        // total_price is optional on GET /quote/:id, whose response table omits it, but required here. The anchor
        // has already created the quote, so the rejection keeps the body: its id is in ResponseBody.
        return await SendAsync<QuoteResponse>(httpRequest, cancellationToken, quote =>
                quote.TotalPrice == null
                    ? "The quote server answered POST /quote without the required 'total_price'."
                    : null)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     <c>GET /quote/:id</c>: fetches a previously issued firm quote. Anchors keep quotes available past their
    ///     expiry. Requires a JWT.
    /// </summary>
    /// <param name="quoteId">The <see cref="QuoteResponse.Id" /> returned by <see cref="PostQuoteAsync" />.</param>
    /// <param name="jwt">The SEP-10 or SEP-45 JWT of the account that requested the quote.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The quote.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="quoteId" /> or <paramref name="jwt" /> is empty, or when
    ///     <paramref name="quoteId" /> is <c>.</c> or <c>..</c>.
    /// </exception>
    /// <exception cref="NotFoundException">Thrown when the anchor has no such quote for the account.</exception>
    /// <exception cref="QuoteServerException">
    ///     Thrown (as one of its subtypes) when the anchor answers with an error or an unusable response, including
    ///     a quote whose <c>id</c> is not <paramref name="quoteId" />.
    /// </exception>
    /// <exception cref="HttpRequestException">Thrown when the request cannot be sent or the response not read.</exception>
    /// <exception cref="TaskCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is canceled, or when the timeout elapses (the inner
    ///     exception is then a <see cref="TimeoutException" />).
    /// </exception>
    public async Task<QuoteResponse> GetQuoteAsync(string quoteId, string jwt,
        CancellationToken cancellationToken = default)
    {
        RequestValidation.RequireNonEmpty(quoteId, nameof(quoteId));
        RequestValidation.RequireNonEmpty(jwt, nameof(jwt));
        // Escaped as a single path segment, so an id containing '/', '?' or '#' cannot reach another endpoint. The
        // dot segments are the exception: EscapeDataString leaves '.' alone and the URI parser then resolves "." and
        // ".." away, so they are refused outright.
        if (quoteId is "." or "..")
        {
            throw new ArgumentException("The quote id must not be a '.' or '..' path segment.", nameof(quoteId));
        }

        var uri = BuildUri("quote/" + Uri.EscapeDataString(quoteId), null);
        // The spec defines the response id as "the id specified in the request".
        return await SendAsync<QuoteResponse>(CreateRequest(HttpMethod.Get, uri, jwt), cancellationToken, quote =>
                string.Equals(quote.Id, quoteId, StringComparison.Ordinal)
                    ? null
                    : $"The quote server answered GET /quote/:id for {UntrustedJsonValue.Describe(quoteId)} with " +
                      $"the quote {UntrustedJsonValue.Describe(quote.Id)}.")
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Disposes the HTTP client if this service created it.
    /// </summary>
    public void Dispose()
    {
        if (_internalHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private string BuildUri(string endpoint, Dictionary<string, string>? queryParameters)
    {
        var uri = $"{_serviceAddress}/{endpoint}";
        if (queryParameters == null || queryParameters.Count == 0)
        {
            return uri;
        }

        return uri + "?" + string.Join("&",
            queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri, string? jwt,
        HttpContent? content = null)
    {
        // Content first, so a custom header that is a content header (Content-Language, ...) has somewhere to go.
        var request = new HttpRequestMessage(method, uri) { Content = content };
        if (_httpRequestHeaders != null)
        {
            foreach (var header in _httpRequestHeaders)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(jwt))
        {
            // Set after the custom headers so the per-call JWT wins over any configured Authorization header.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        return request;
    }

    /// <param name="request">The request; disposed when the call completes.</param>
    /// <param name="cancellationToken">The caller's token.</param>
    /// <param name="validate">
    ///     Optional check of the parsed response, returning why it is unusable or <see langword="null" />. A
    ///     rejection keeps the response's status code and body, so a caller can still recover what the anchor sent
    ///     (the id of a firm quote it already created, for example).
    /// </param>
    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken,
        Func<T, string?>? validate = null)
        where T : class
    {
        using (request)
        {
            // ResponseHeadersRead: the default (ResponseContentRead) buffers the entire body inside HttpClient
            // before returning, which would make the size limit below a check after the memory was already spent.
            // It also takes the body out of HttpClient.Timeout's reach, so the same deadline is re-imposed here over
            // the whole exchange; without it a server that trickles its body would hold the call open indefinitely.
            var timeout = GetTimeout();
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (timeout != Timeout.InfiniteTimeSpan)
            {
                timeoutSource.CancelAfter(timeout);
            }

            try
            {
                using var response = await _httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutSource.Token)
                    .ConfigureAwait(false);
                var statusCode = (int)response.StatusCode;
                byte[]? body;
                try
                {
                    body = await ReadBodyBoundedAsync(response, statusCode, timeoutSource.Token)
                        .ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is IOException or InvalidDataException)
                {
                    // A connection that drops mid-body (IOException), or a compressed body a decompressing handler
                    // cannot inflate (InvalidDataException). HttpClient itself reports both as HttpRequestException
                    // when it buffers the body; reading the stream directly surfaces the raw exception instead.
                    if (response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Error while reading the SEP-0038 response body.", ex);
                    }

                    // The status alone still selects the exception type; only the anchor's error text is lost.
                    body = null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw CreateErrorException(response.StatusCode, body);
                }

                T? result;
                try
                {
                    result = JsonSerializer.Deserialize<T>(body!, JsonOptions.DefaultOptions);
                }
                catch (JsonException ex)
                {
                    throw new UnexpectedResponseException(
                        $"The quote server returned HTTP {statusCode} with a body that is not a valid {typeof(T).Name}: " +
                        ex.Message,
                        statusCode, null, DecodeBody(body!), ex);
                }

                if (result == null)
                {
                    throw new UnexpectedResponseException(
                        $"The quote server returned HTTP {statusCode} with a null body.", statusCode, null,
                        DecodeBody(body!));
                }

                var problem = validate?.Invoke(result);
                if (problem != null)
                {
                    throw new UnexpectedResponseException(problem, statusCode, null, DecodeBody(body!));
                }

                return result;
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested &&
                                                       timeoutSource.IsCancellationRequested)
            {
                throw CreateTimeoutException(timeout, ex);
            }
            catch (TimeoutRejectedException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw CreateTimeoutException(ex.Timeout, ex);
            }
        }
    }

    /// <summary>
    ///     The deadline for one whole exchange: the client's <see cref="HttpClient.Timeout" />, tightened by the
    ///     internal client's <see cref="HttpResilienceOptions.RequestTimeout" />. Read on every call, because a caller
    ///     may change the timeout of a client it shares with this service.
    /// </summary>
    private TimeSpan GetTimeout()
    {
        var timeout = _httpClient.Timeout;
        if (_requestTimeout is { } requestTimeout &&
            (timeout == Timeout.InfiniteTimeSpan || requestTimeout < timeout))
        {
            timeout = requestTimeout;
        }

        return timeout;
    }

    private static TaskCanceledException CreateTimeoutException(TimeSpan timeout, Exception cause)
    {
        var seconds = timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        return new TaskCanceledException(
            $"The SEP-0038 request was canceled due to the configured timeout of {seconds} seconds elapsing.",
            new TimeoutException(cause.Message, cause));
    }

    /// <summary>
    ///     Reads the response body with an upper size bound of <see cref="MaxResponseBodyBytes" />.
    /// </summary>
    private static async Task<byte[]> ReadBodyBoundedAsync(HttpResponseMessage response, int statusCode,
        CancellationToken cancellationToken)
    {
        if (response.Content == null)
        {
            return Array.Empty<byte>();
        }

        if (response.Content.Headers.ContentLength is long contentLength && contentLength > MaxResponseBodyBytes)
        {
            throw TooLarge(statusCode);
        }

#if NETSTANDARD2_1
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            // Enforced on the bytes actually read too: Content-Length can be absent (chunked) or understated.
            if (buffer.Length + read > MaxResponseBodyBytes)
            {
                throw TooLarge(statusCode);
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static UnexpectedResponseException TooLarge(int statusCode)
    {
        return new UnexpectedResponseException(
            $"The quote server returned HTTP {statusCode} with a body larger than the {MaxResponseBodyBytes}-byte limit.",
            statusCode, null, null);
    }

    private static QuoteServerException CreateErrorException(HttpStatusCode statusCode, byte[]? body)
    {
        var text = body == null ? null : DecodeBody(body);
        var error = body == null ? null : TryReadError(body);
        var code = (int)statusCode;
        var message = $"The quote server returned HTTP {code}" +
                      (error == null ? "." : $": {UntrustedJsonValue.Describe(error, MaxEchoedErrorLength)}");
        return statusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestException(message, error, text),
            HttpStatusCode.Forbidden => new PermissionDeniedException(message, error, text),
            HttpStatusCode.NotFound => new NotFoundException(message, error, text),
            _ => new UnexpectedResponseException(message, code, error, text),
        };
    }

    /// <summary>
    ///     Returns the string <c>error</c> field of a SEP-38 error body, or null when the body is not a JSON object
    ///     with one. Duplicated properties make the body malformed, as everywhere else in the SDK.
    /// </summary>
    private static string? TryReadError(byte[] body)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions { AllowDuplicateProperties = false });
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.String)
            {
                return error.GetString();
            }
        }
        catch (JsonException)
        {
            // Not JSON (an HTML error page from a proxy, for example): the status code is all there is.
        }
        catch (InvalidOperationException)
        {
            // Grammatical JSON whose error string cannot be decoded (a lone surrogate escape, invalid UTF-8): the
            // status code still selects the exception type.
        }

        return null;
    }

    private static string DecodeBody(byte[] body)
    {
        return Encoding.UTF8.GetString(body);
    }
}
