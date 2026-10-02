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
///         keeping the scale given; responses are read exactly: a value that <see cref="decimal" /> cannot hold without
///         rounding fails with <see cref="UnexpectedResponseException" /> rather than being approximated.
///     </para>
///     <para>
///         <b>Errors</b>
///     </para>
///     <para>
///         Invalid arguments — including the sell/buy amount exclusivity rules, a zero or negative amount, a string
///         property that is empty or whitespace rather than null, and a JWT that is not printable ASCII without
///         whitespace — throw <see cref="ArgumentException" /> from the returned task, before anything is sent. Error
///         responses map to <see cref="BadRequestException" /> (400), <see cref="PermissionDeniedException" /> (403),
///         <see cref="QuoteServerNotFoundException" /> (404) and <see cref="UnexpectedResponseException" /> (anything
///         else, including a 3xx, or an unusable success body); all derive from <see cref="QuoteServerException" />.
///         A <c>Retry-After</c> header on an error response (typically 429 or 503) is exposed as
///         <see cref="UnexpectedResponseException" />'s <see cref="QuoteServerException.RetryAfterDelay" />.
///         Response bodies are read up to 1 MiB, and parsed with the SDK's hardened JSON options, which reject
///         duplicated mapped properties. An error body over the limit is dropped (<see cref="QuoteServerException.Error" />
///         and <see cref="QuoteServerException.ResponseBody" /> are then null) but the status still selects the
///         exception type; a success body over the limit raises <see cref="UnexpectedResponseException" />. A leading
///         UTF-8 byte order mark is skipped.
///     </para>
///     <para>
///         A success body is also checked against the request: a firm quote must carry a usable <c>id</c>, the
///         requested asset pair, no delivery method other than the one requested (compared without regard to case),
///         and an expiry no earlier than the requested <c>expire_after</c>; prices and amounts must be greater than
///         zero, and a <c>decimals</c> count must not be negative. A response that fails these checks raises
///         <see cref="UnexpectedResponseException" />, whose <see cref="QuoteServerException.ResponseBody" /> still
///         holds what the anchor sent (for <c>POST /quote</c>, the id of the quote it already created).
///     </para>
///     <para>
///         Transport failures surface as <see cref="HttpRequestException" /> (including a connection that drops
///         while a success body is being read) or <see cref="TaskCanceledException" /> (cancellation, or a timeout, in
///         which case its inner exception is a <see cref="TimeoutException" />). The whole exchange — headers and
///         body — is bounded by the <see cref="HttpClient.Timeout" /> of the client in use (and, for the internal
///         client, by <see cref="HttpResilienceOptions.RequestTimeout" />), so a server that trickles its body cannot
///         stall the caller. An internal client with <see cref="HttpResilienceOptions.EnableCircuitBreaker" /> set
///         throws Polly's <c>BrokenCircuitException</c> while the circuit is open, and a call on a disposed service
///         that owns its client throws <see cref="ObjectDisposedException" />. On runtimes before .NET 10, a request
///         whose URL exceeds the runtime's length limit (a quote id tens of kilobytes long) raises
///         <see cref="UriFormatException" />.
///     </para>
///     <para>
///         <b>Redirects</b>
///     </para>
///     <para>
///         The internal client does not follow redirects, so a 3xx answer raises
///         <see cref="UnexpectedResponseException" /> and a request is never replayed to another host. A caller-owned
///         client follows redirects if its handler does; the service then rejects any response that comes from a
///         different origin than the quote server, but by that point the request, its body and the custom headers
///         have already been sent there (on .NET's <c>SocketsHttpHandler</c>, not the JWT, whose <c>Authorization</c>
///         header it drops on a redirect). Only the final location is visible, so a chain that leaves the origin and
///         returns to it is not detected at all. Disable <c>AllowAutoRedirect</c> on a caller-owned client's handler
///         to avoid both. The check compares the final request URI, so a caller's handler that rewrites it to
///         another origin (a gateway, for example) is rejected the same way.
///     </para>
///     <para>
///         <b>Retries</b>
///     </para>
///     <para>
///         <c>POST /quote</c> is not idempotent: every call creates a new firm quote that the anchor holds in reserve
///         until it expires. Do not pass <see cref="HttpResilienceOptionsPresets.ForHorizon" /> or
///         <see cref="HttpResilienceOptionsPresets.ForSoroban" />: both retry <c>POST</c> on a 408, 429, 500, 502,
///         503 or 504 answer, so one call can reserve two quotes. Use <see cref="HttpResilienceOptionsPresets.WithConnectionRetries" /> (which
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
    /// <param name="httpRequestHeaders">
    ///     Optional headers added to every request. A content header (<c>Content-Language</c>, for example) is sent
    ///     only with <c>POST /quote</c>; the other requests have no body to carry it, so it is dropped from them. A
    ///     <c>Content-Type</c> header is ignored: <c>POST /quote</c> always sends <c>application/json</c>. An
    ///     <c>Accept</c> header is ignored too: every request asks for <c>application/json</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="serviceAddress" /> is empty, is not an absolute URL, carries a query,
    ///     fragment or user information, or uses plain <c>http</c> for a host other than a loopback address (the JWT travels with every
    ///     authenticated request, so it must not go in cleartext; loopback is allowed for local development); or when
    ///     <paramref name="httpRequestHeaders" /> holds a name that is not a valid HTTP header name, a
    ///     <c>Content-Length</c> or <c>Transfer-Encoding</c> header, or a value with anything other than printable
    ///     ASCII, spaces and tabs (a line break would otherwise split into extra headers on the wire). The headers are
    ///     copied, so later changes to the dictionary have no effect.
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

        ValidateHeaders(httpRequestHeaders, nameof(httpRequestHeaders));

        // The address as parsed, not as given: Uri drops surrounding whitespace that would otherwise reach every
        // request URI and fail there.
        _serviceAddress = new Uri(serviceAddress, UriKind.Absolute).AbsoluteUri.TrimEnd('/');
        // A copy, so the checks above still hold if the caller changes its dictionary later.
        _httpRequestHeaders = httpRequestHeaders == null ? null : new Dictionary<string, string>(httpRequestHeaders);
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _internalHttpClient = false;
        }
        else
        {
            // Not following redirects: a 3xx would otherwise replay the request, POST body and custom headers
            // included, to whatever host the Location names.
            _httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions,
                innerHandler: CreateNonRedirectingHandler());
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
    ///     Optional retry and timeout options for the stellar.toml fetch and the client the service creates. Ignored
    ///     when <paramref name="httpClient" /> is provided.
    /// </param>
    /// <param name="bearerToken">
    ///     Optional bearer token for the stellar.toml fetch only. Ignored when <paramref name="httpClient" /> is
    ///     provided: the stellar.toml is then fetched with the caller's client as it is configured, without the token.
    /// </param>
    /// <param name="httpClient">Optional HTTP client to use; it remains owned by the caller.</param>
    /// <param name="httpRequestHeaders">
    ///     Optional headers added to the stellar.toml fetch and every request. A content header is sent only with
    ///     <c>POST /quote</c>, the one request with a body, and a <c>Content-Type</c> header is ignored. An
    ///     <c>Accept</c> header reaches the stellar.toml fetch only: requests to the quote server always ask for
    ///     <c>application/json</c>.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the stellar.toml fetch.</param>
    /// <returns>A client for the anchor's quote server.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="domain" /> is null or empty, or when <paramref name="httpRequestHeaders" />
    ///     holds a header the constructor refuses.
    /// </exception>
    /// <exception cref="UriFormatException">Thrown when <paramref name="domain" /> is not a valid host name.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is canceled during the stellar.toml fetch.
    /// </exception>
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
        // Before the stellar.toml fetch, which sends the same headers.
        ValidateHeaders(httpRequestHeaders, nameof(httpRequestHeaders));
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
    ///     Rejects custom headers that <see cref="HttpHeaders.TryAddWithoutValidation(string, string)" /> would
    ///     otherwise drop silently (an invalid name), send as more than one header (a line break in the value), or
    ///     let fail only at send time (a non-ASCII value, a framing header).
    /// </summary>
    /// <param name="headers">The custom headers to check.</param>
    /// <param name="paramName">The caller's parameter name, reported as <see cref="ArgumentException.ParamName" />.</param>
    /// <exception cref="ArgumentException">Thrown when a header name or value is unusable.</exception>
    private static void ValidateHeaders(Dictionary<string, string>? headers, string paramName)
    {
        if (headers == null)
        {
            return;
        }

        using var probe = new HttpRequestMessage();
        using var probeContent = new ByteArrayContent(Array.Empty<byte>());
        foreach (var header in headers)
        {
            // A name is usable when it is valid as either a request or a content header.
            if (string.IsNullOrWhiteSpace(header.Key) ||
                (!probe.Headers.TryAddWithoutValidation(header.Key, "x") &&
                 !probeContent.Headers.TryAddWithoutValidation(header.Key, "x")))
            {
                throw new ArgumentException(
                    $"The custom header name {UntrustedJsonValue.Describe(header.Key)} is not a valid HTTP header name.",
                    paramName);
            }

            // The service frames the body itself; a caller's value would contradict it and fail at send time.
            if (string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(header.Key, "Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"The custom header {UntrustedJsonValue.Describe(header.Key)} is set by the service and cannot be " +
                    "supplied.", paramName);
            }

            // Printable ASCII, space and tab only: a line break would split into extra headers on the wire, and a
            // non-ASCII character fails at send time as an HttpRequestException that looks like a transport fault.
            foreach (var c in header.Value ?? string.Empty)
            {
                if ((c < ' ' && c != '\t') || c > '~')
                {
                    throw new ArgumentException(
                        $"The value of the custom header {UntrustedJsonValue.Describe(header.Key)} must consist of " +
                        "printable ASCII characters, spaces and tabs.", paramName);
                }
            }
        }
    }

    private static HttpMessageHandler CreateNonRedirectingHandler()
    {
#if NET8_0_OR_GREATER
        return new SocketsHttpHandler { AllowAutoRedirect = false };
#else
        // SocketsHttpHandler is not available on netstandard2.1 reference assemblies.
        return new HttpClientHandler { AllowAutoRedirect = false };
#endif
    }

    /// <summary>
    ///     <c>GET /info</c>: the Stellar and off-chain assets available for trading, with their delivery methods and
    ///     country codes.
    /// </summary>
    /// <param name="jwt">Optional SEP-10 or SEP-45 JWT; the anchor may use it to personalize the response.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The supported assets.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="jwt" /> is empty or whitespace rather than null, or is not printable ASCII
    ///     without whitespace.
    /// </exception>
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
        RequestValidation.RequireValidJwt(jwt, nameof(jwt), false);
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
    /// <exception cref="ArgumentException">
    ///     Thrown when the request mixes or omits the sell and buy sides, when a string property is empty or
    ///     whitespace rather than null, or when the JWT is not printable ASCII without whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is zero or negative.</exception>
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
        var sellSide = request.SellAsset != null;
        return await SendAsync<PricesResponse>(httpRequest, cancellationToken, response =>
            {
                var prices = sellSide ? response.BuyAssets : response.SellAssets;
                if (prices == null)
                {
                    return $"The quote server answered a {(sellSide ? "sell" : "buy")}-side GET /prices without " +
                           $"'{(sellSide ? "buy_assets" : "sell_assets")}'.";
                }

                foreach (var price in prices)
                {
                    if (price.Price <= 0)
                    {
                        return $"The quote server answered GET /prices with a price of {price.Price} for " +
                               $"{UntrustedJsonValue.Describe(price.Asset)}; prices must be greater than zero.";
                    }

                    if (price.Decimals < 0)
                    {
                        return $"The quote server answered GET /prices with {price.Decimals} decimals for " +
                               $"{UntrustedJsonValue.Describe(price.Asset)}; the number of decimals must not be " +
                               "negative.";
                    }
                }

                return null;
            })
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
    ///     Thrown when an asset is missing, when not exactly one amount is given, when the context is SEP-24, when an
    ///     optional string property is empty or whitespace rather than null, or when the JWT is not printable ASCII
    ///     without whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the context is not a defined value, or the amount is zero or negative.
    /// </exception>
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
        return await SendAsync<PriceResponse>(httpRequest, cancellationToken, price =>
                CheckPositive("GET /price", price.TotalPrice, price.Price, price.SellAmount, price.BuyAmount))
            .ConfigureAwait(false);
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
    ///     Thrown when an asset or the JWT is missing, when the JWT is not printable ASCII without whitespace, when
    ///     not exactly one amount is given, when an optional string property is empty or whitespace rather than
    ///     null, or when both delivery methods are given.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the context is not a defined value, or the amount is zero or negative.
    /// </exception>
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
        // Sent as the UTF-8 bytes the writer produced: a string in between would decode and re-encode them.
        var body = new ByteArrayContent(request.ToJson());
        body.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        var httpRequest = CreateRequest(HttpMethod.Post, BuildUri("quote", null), request.Jwt, body);
        // The anchor has already created the quote, so a rejection keeps the body: its id is in ResponseBody.
        return await SendAsync<QuoteResponse>(httpRequest, cancellationToken, quote => CheckPostedQuote(request, quote))
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
    ///     Thrown when <paramref name="quoteId" /> or <paramref name="jwt" /> is empty, when
    ///     <paramref name="quoteId" /> is <c>.</c> or <c>..</c>, or when <paramref name="jwt" /> is not printable ASCII
    ///     without whitespace.
    /// </exception>
    /// <exception cref="QuoteServerNotFoundException">Thrown when the anchor has no such quote for the account.</exception>
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
        RequestValidation.RequireValidJwt(jwt, nameof(jwt), true);
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
                    ? CheckPositive("GET /quote/:id", quote.TotalPrice, quote.Price, quote.SellAmount, quote.BuyAmount)
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
                // Accept is set below; a configured value would go out next to it ("text/html, application/json"),
                // and an anchor may then answer with something other than JSON.
                if (string.Equals(header.Key, "Accept", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // A content header the body already carries (its Content-Type) is not added again: a second value
                // would make the header malformed ("application/json; charset=utf-8, text/plain").
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value) && content != null &&
                    !content.Headers.TryGetValues(header.Key, out _))
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (jwt != null)
        {
            // Validated by the caller (RequestValidation.RequireValidJwt). Set after the custom headers so the per-call JWT wins over any configured Authorization header.
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
            // Captured before sending: a client that follows redirects updates the request's URI to the final one,
            // and turns a POST into a GET on a 301 or 302.
            var requestUri = request.RequestUri!;
            var method = request.Method.Method;
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
                var finalUri = response.RequestMessage?.RequestUri ?? requestUri;
                if (Uri.Compare(finalUri, requestUri, UriComponents.SchemeAndServer, UriFormat.UriEscaped,
                        StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new UnexpectedResponseException(
                        $"The SEP-0038 {method} request was redirected to a different origin; the " +
                        "response was rejected.", statusCode, null, null);
                }

                // Null when the body is over the size limit.
                ReadOnlyMemory<byte>? body;
                try
                {
                    body = await ReadBodyBoundedAsync(response, timeoutSource.Token).ConfigureAwait(false);
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
                    // An error body that is missing, unreadable or over the limit still leaves the status to select
                    // the exception type, and the Retry-After header to tell the caller when to come back.
                    throw CreateErrorException(response.StatusCode, body, ReadRetryAfter(response));
                }

                if (body is not { } json)
                {
                    throw TooLarge(statusCode);
                }

                T? result;
                try
                {
                    result = JsonSerializer.Deserialize<T>(json.Span, JsonOptions.DefaultOptions);
                }
                catch (JsonException ex)
                {
                    throw new UnexpectedResponseException(
                        $"The quote server returned HTTP {statusCode} with a body that is not a valid {typeof(T).Name}: " +
                        ex.Message,
                        statusCode, null, DecodeBody(json), ex);
                }

                if (result == null)
                {
                    throw new UnexpectedResponseException(
                        $"The quote server returned HTTP {statusCode} with a null body.", statusCode, null,
                        DecodeBody(json));
                }

                var problem = validate?.Invoke(result);
                if (problem != null)
                {
                    throw new UnexpectedResponseException(problem, statusCode, null, DecodeBody(json));
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
    ///     Reads the response body, or returns <see langword="null" /> when it exceeds
    ///     <see cref="MaxResponseBodyBytes" />. A leading UTF-8 byte order mark is skipped, as
    ///     <see cref="HttpContent.ReadAsStringAsync()" /> would; the JSON parsers reject it.
    /// </summary>
    private static async Task<ReadOnlyMemory<byte>?> ReadBodyBoundedAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.Content == null)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        if (response.Content.Headers.ContentLength is long contentLength && contentLength > MaxResponseBodyBytes)
        {
            return null;
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
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        // A view over the stream's own buffer, not a copy; disposing a MemoryStream leaves its buffer intact.
        var bytes = buffer.GetBuffer();
        var length = (int)buffer.Length;
        var offset = length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        return new ReadOnlyMemory<byte>(bytes, offset, length - offset);
    }

    private static UnexpectedResponseException TooLarge(int statusCode)
    {
        return new UnexpectedResponseException(
            $"The quote server returned HTTP {statusCode} with a body larger than the {MaxResponseBodyBytes}-byte limit.",
            statusCode, null, null);
    }

    /// <summary>
    ///     Checks a <c>POST /quote</c> answer against its request, returning why it is unusable or
    ///     <see langword="null" />.
    /// </summary>
    private static string? CheckPostedQuote(QuoteRequest request, QuoteResponse quote)
    {
        // The id is what the caller passes on as quote_id and to GetQuoteAsync, which refuses these same values.
        if (string.IsNullOrWhiteSpace(quote.Id) || quote.Id is "." or "..")
        {
            return $"The quote server answered POST /quote with the unusable quote id {UntrustedJsonValue.Describe(quote.Id)}.";
        }

        // total_price is optional on GET /quote/:id, whose response table omits it, but required here.
        if (quote.TotalPrice == null)
        {
            return "The quote server answered POST /quote without the required 'total_price'.";
        }

        if (!string.Equals(quote.SellAsset, request.SellAsset, StringComparison.Ordinal) ||
            !string.Equals(quote.BuyAsset, request.BuyAsset, StringComparison.Ordinal))
        {
            // A stellar:CODE:ISSUER identifier runs past Describe's default 64-unit bound.
            return $"The quote server answered POST /quote for {Echo(request.SellAsset)} -> {Echo(request.BuyAsset)} " +
                   $"with a quote for {Echo(quote.SellAsset)} -> {Echo(quote.BuyAsset)}.";
        }

        // The specification echoes a delivery method "only if specified in the request". Case is ignored: an anchor
        // that matched the requested name to the one it lists may echo the listed spelling ("pix" -> "PIX").
        if ((quote.SellDeliveryMethod != null &&
             !string.Equals(quote.SellDeliveryMethod, request.SellDeliveryMethod,
                 StringComparison.OrdinalIgnoreCase)) ||
            (quote.BuyDeliveryMethod != null &&
             !string.Equals(quote.BuyDeliveryMethod, request.BuyDeliveryMethod, StringComparison.OrdinalIgnoreCase)))
        {
            return "The quote server answered POST /quote with a delivery method the request did not ask for.";
        }

        // An anchor that cannot honor expire_after must answer 400 instead. Compared with the value sent, which is
        // rounded up to the whole second, so an anchor that stores whole seconds can match it exactly.
        if (request.SentExpireAfter is { } expireAfter && quote.ExpiresAt < expireAfter)
        {
            return "The quote server answered POST /quote with a quote that expires before the requested " +
                   "'expire_after'.";
        }

        return CheckPositive("POST /quote", quote.TotalPrice, quote.Price, quote.SellAmount, quote.BuyAmount);
    }

    /// <summary>
    ///     Reads the <c>Retry-After</c> delay of <paramref name="response" />, or <see langword="null" />.
    /// </summary>
    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        // The typed header is null for forms .NET cannot represent (delta-seconds beyond int.MaxValue, an ISO 8601
        // date). The raw value is parsed then, as RetryingHttpMessageHandler and the Horizon exceptions do.
        return RetryAfterParser.ToTimeSpan(response.Headers.RetryAfter) ??
               (response.Headers.TryGetValues("Retry-After", out var values)
                   ? RetryAfterParser.Parse(values.FirstOrDefault())
                   : null);
    }

    private static string Echo(string? value)
    {
        return UntrustedJsonValue.Describe(value, MaxEchoedErrorLength);
    }

    /// <summary>
    ///     Returns why a price or quote is unusable when one of its prices or amounts is zero or negative, or
    ///     <see langword="null" />. Requests refuse such values too. A null <paramref name="totalPrice" /> passes:
    ///     <c>GET /quote/:id</c> may omit it, and the lifted <c>&lt;=</c> is false for null.
    /// </summary>
    private static string? CheckPositive(string endpoint, decimal? totalPrice, decimal price, decimal sellAmount,
        decimal buyAmount)
    {
        if (totalPrice <= 0 || price <= 0 || sellAmount <= 0 || buyAmount <= 0)
        {
            return $"The quote server answered {endpoint} with a price or amount that is not greater than zero.";
        }

        return null;
    }

    private static QuoteServerException CreateErrorException(HttpStatusCode statusCode, ReadOnlyMemory<byte>? body,
        TimeSpan? retryAfterDelay)
    {
        var text = body == null ? null : DecodeBody(body.Value);
        var error = body == null ? null : TryReadError(body.Value);
        var code = (int)statusCode;
        var message = $"The quote server returned HTTP {code}" +
                      (error == null ? "." : $": {UntrustedJsonValue.Describe(error, MaxEchoedErrorLength)}");
        return statusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestException(message, error, text),
            HttpStatusCode.Forbidden => new PermissionDeniedException(message, error, text),
            HttpStatusCode.NotFound => new QuoteServerNotFoundException(message, error, text),
            _ => new UnexpectedResponseException(message, code, error, text, retryAfterDelay: retryAfterDelay),
        };
    }

    /// <summary>
    ///     Returns the string <c>error</c> field of a SEP-38 error body, or null when the body is not a JSON object
    ///     with one. Duplicated properties make the body malformed, as everywhere else in the SDK.
    /// </summary>
    private static string? TryReadError(ReadOnlyMemory<byte> body)
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

    private static string DecodeBody(ReadOnlyMemory<byte> body)
    {
        return Encoding.UTF8.GetString(body.Span);
    }
}
