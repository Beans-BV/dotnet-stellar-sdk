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
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0001.Exceptions;
using StellarDotnetSdk.Sep.Sep0012.Exceptions;
using StellarDotnetSdk.Sep.Sep0012.Requests;
using StellarDotnetSdk.Sep.Sep0012.Responses;

namespace StellarDotnetSdk.Sep.Sep0012;

/// <summary>
///     Implements the client side of SEP-0012 v1.15.0 - KYC API. SEP-0012 is how a wallet uploads KYC (or other)
///     customer information to an anchor once and reuses it across services: it supplies the customer data that
///     SEP-6, SEP-24 and SEP-31 anchor flows ask for, checks its review status, and lets the customer erase it.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0012.md">SEP-0012</a>.
/// </summary>
/// <remarks>
///     <para>
///         <b>Discovery and authentication.</b> <see cref="FromDomainAsync" /> reads the anchor's stellar.toml and uses
///         <c>KYC_SERVER</c>, falling back to <c>TRANSFER_SERVER</c> as SEP-0012 prescribes. Every endpoint requires a
///         JWT obtained from the same anchor through SEP-10 (<see cref="Sep0010.ClientWebAuth" />, for <c>G...</c> and
///         <c>M...</c> accounts) or SEP-45 (<see cref="Sep0045.ClientWebAuthContract" />, for <c>C...</c> contract
///         accounts); each request type carries it in its <c>Jwt</c> property. Because every request carries that JWT
///         and customer data, the KYC server must use <c>https</c>; plain <c>http</c> is accepted only for
///         <c>localhost</c> or a loopback IP address in its standard form (such as <c>127.0.0.1</c> or <c>[::1]</c>).
///         The same rule applies to a callback URL registered with <see cref="PutCustomerCallbackAsync" />.
///     </para>
///     <para>
///         <b>Response handling.</b> Response bodies are read with a hard size limit of 1 MiB, so a hostile or
///         malfunctioning server cannot exhaust memory by streaming an unbounded body, and the whole exchange —
///         headers and body — is bounded by the <see cref="HttpClient.Timeout" /> of the client in use (and, for the
///         internal client, by <see cref="HttpResilienceOptions.RequestTimeout" />), so it cannot stall the caller
///         either. Bodies are decoded as strict UTF-8, so malformed bytes are rejected rather than replaced, and are
///         parsed with <see cref="JsonOptions.DefaultOptions" /> — duplicate properties are rejected, including in
///         error bodies — and statuses and field types are matched against the exact literal sets SEP-0012
///         defines. A success response that fails any of this raises
///         <see cref="InvalidKycResponseException" />. Error statuses raise <see cref="AuthenticationRequiredException" />
///         (401, or 403 <c>authentication_required</c>), <see cref="CustomerNotFoundException" /> (404 from
///         <c>GET</c>/<c>PUT /customer</c>, <c>PUT /customer/verification</c>, <c>PUT /customer/callback</c> or
///         <c>DELETE /customer</c>), <see cref="PayloadTooLargeException" /> (413) or <see cref="KycServiceException" />
///         (anything else, including a 404 from the <c>/customer/files</c> endpoints); each carries the anchor's
///         <c>error</c> text when the error body holds one.
///     </para>
///     <para>
///         <b>Transport failures</b> surface as <see cref="HttpRequestException" /> (including a connection that drops
///         while a success body is being read) or <see cref="TaskCanceledException" /> (cancellation, or a timeout, in
///         which case its inner exception is a <see cref="TimeoutException" />). A call on a disposed service throws
///         <see cref="ObjectDisposedException" /> once its arguments have passed validation. On .NET 8, a request
///         whose URL exceeds the runtime's length limit (an identifier tens of kilobytes long) raises
///         <see cref="UriFormatException" />.
///     </para>
///     <para>
///         <b>Redirects.</b> The internal client does not follow redirects, so a 3xx answer raises
///         <see cref="KycServiceException" /> and the request body is never replayed to another host. An external
///         client follows redirects if its handler does; the service then rejects any response that comes from a
///         different origin than <see cref="ServiceAddress" />, but by that point the request, including its customer
///         data, has already been sent there. It also rejects a response to a request the handler resent with a
///         different method (a <c>POST</c> after a 301, 302 or 303, or a <c>PUT</c> or <c>DELETE</c> after a 303,
///         is resent as <c>GET</c>), whatever its status: a success would not show the change was applied, and an
///         error would answer a request the caller never made, so both raise a plain
///         <see cref="KycServiceException" />. Only the final location is visible, so a chain that leaves the origin
///         and returns to it is not detected at all. Disable <c>AllowAutoRedirect</c> on an external client's handler
///         to avoid both.
///     </para>
///     <para>
///         <b>HttpClient ownership.</b> Pass a shared, long-lived <see cref="HttpClient" /> in production. When none is
///         given, the service creates an internal one (honouring <see cref="HttpResilienceOptions" />) and disposes it
///         in <see cref="Dispose" />; an external client is never disposed by this service.
///     </para>
/// </remarks>
public class KycService : IDisposable
{
    /// <summary>
    ///     Upper bound on a response body, in bytes. The largest legitimate SEP-0012 bodies — a <c>GET /customer</c>
    ///     listing every SEP-0009 field with choices, or a long <c>GET /customer/files</c> list — are a few tens of
    ///     kilobytes, so 1 MiB is ample headroom while still bounding what a hostile server can make the client buffer.
    /// </summary>
    internal const int MaxResponseBodyBytes = 1024 * 1024;

    private const string AuthenticationRequiredType = "authentication_required";
    private const string VerificationSuffix = "_verification";
    private const string FileIdSuffix = "_file_id";
    private static readonly string[] MemoTypes = { "text", "id", "hash" };

    // Throws on malformed input instead of substituting U+FFFD, which would turn corrupt bytes into a valid string.
    // Used for success bodies only: an error body stays lenient, so its "type" still selects the exception.
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly bool _ownsHttpClient;
    private readonly TimeSpan? _requestTimeout;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KycService" /> class for an explicit KYC server URL. Use
    ///     <see cref="FromDomainAsync" /> to discover the URL from the anchor's stellar.toml instead.
    /// </summary>
    /// <param name="serviceAddress">
    ///     The absolute <c>https</c> URL of the anchor's KYC server (its <c>KYC_SERVER</c>), without a query, fragment
    ///     or user info. Plain <c>http</c> is accepted only for <c>localhost</c> or a loopback IP address in its
    ///     standard form, for local testing.
    /// </param>
    /// <param name="httpClient">
    ///     Optional shared HTTP client. If <c>null</c>, an internal client is created and disposed with this instance.
    /// </param>
    /// <param name="resilienceOptions">
    ///     Optional retry/timeout options for the internal client. Ignored when <paramref name="httpClient" /> is given.
    ///     Do not pass options that retry <c>POST</c> requests on a status code, such as
    ///     <see cref="HttpResilienceOptionsPresets.ForHorizon" /> or <see cref="HttpResilienceOptionsPresets.ForSoroban" />:
    ///     <c>POST /customer/files</c> creates a new file on every attempt, so a retry after the anchor already stored
    ///     the upload leaves a duplicate behind. Connection-failure retries apply to every method (see
    ///     <see cref="HttpResilienceOptionsPresets.WithConnectionRetries" />), so they carry the same risk when a
    ///     connection drops after the upload was received.
    /// </param>
    /// <param name="httpRequestHeaders">
    ///     Optional custom headers added to every request. A header the service sets itself (<c>Authorization</c>, and
    ///     the <c>Content-Type</c> of a request body) is never overridden by it, and a name that is not a valid HTTP
    ///     header name is ignored. Do not pass <c>Content-Length</c>: the body's length is computed per request.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="serviceAddress" /> is empty, is not an absolute http(s) URL, uses <c>http</c>
    ///     for a host other than <c>localhost</c> or a standard-form loopback IP address, or contains a query,
    ///     fragment or user info.
    /// </exception>
    public KycService(
        string serviceAddress,
        HttpClient? httpClient = null,
        HttpResilienceOptions? resilienceOptions = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        if (serviceAddress == null)
        {
            throw new ArgumentNullException(nameof(serviceAddress));
        }

        if (!TryParseHttpUrl(serviceAddress, out var address, out var uri))
        {
            throw new ArgumentException("The KYC service address must be an absolute http(s) URL.",
                nameof(serviceAddress));
        }

        if (uri.Scheme == Uri.UriSchemeHttp && !IsLoopbackHost(uri, address))
        {
            throw new ArgumentException(
                "The KYC service address must use https: every SEP-0012 request carries a JWT and customer data. " +
                "Plain http is accepted only for localhost or a loopback IP address in its standard form, " +
                "such as 127.0.0.1 or [::1].",
                nameof(serviceAddress));
        }

        // Request paths are appended to the address as text, so a query or fragment would swallow them.
        if (address.IndexOfAny(new[] { '?', '#' }) >= 0 || uri.UserInfo.Length > 0)
        {
            throw new ArgumentException(
                "The KYC service address must not contain a query, fragment or user info.",
                nameof(serviceAddress));
        }

        ServiceAddress = address.TrimEnd('/');
        _httpRequestHeaders = httpRequestHeaders;
        if (httpClient != null)
        {
            _httpClient = httpClient;
        }
        else
        {
            _httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions,
                innerHandler: CreateNonRedirectingHandler());
            _requestTimeout = resilienceOptions?.RequestTimeout;
            _ownsHttpClient = true;
        }
    }

    /// <summary>
    ///     Gets the base URL of the KYC server, without a trailing slash.
    /// </summary>
    public string ServiceAddress { get; }

    /// <summary>
    ///     Disposes the internal <see cref="HttpClient" /> if this instance created it. An externally supplied client
    ///     is left untouched. Any later call on this instance whose arguments pass validation throws
    ///     <see cref="ObjectDisposedException" />.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Flag first, so a call that checks it from here on reports this service as disposed rather than its
        // HttpClient. A call already past the check can still reach the disposed client.
        _disposed = true;
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    ///     Creates a <see cref="KycService" /> for an anchor by reading its stellar.toml (SEP-1): the
    ///     <c>KYC_SERVER</c> if declared, otherwise the <c>TRANSFER_SERVER</c>.
    /// </summary>
    /// <param name="domain">The anchor's home domain, for example <c>testanchor.stellar.org</c>.</param>
    /// <param name="resilienceOptions">
    ///     Optional retry/timeout options, used for the stellar.toml fetch and for the service's internal client —
    ///     both only when no <paramref name="httpClient" /> is given. See the constructor for why options that retry
    ///     <c>POST</c> requests should not be used here.
    /// </param>
    /// <param name="bearerToken">
    ///     Optional bearer token for the stellar.toml fetch only, and only when no <paramref name="httpClient" /> is
    ///     given (an external client's own headers are used as they are).
    /// </param>
    /// <param name="httpClient">
    ///     Optional shared HTTP client, used for the stellar.toml fetch and every SEP-0012 request. If <c>null</c>, the
    ///     returned instance creates and owns an internal one.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom headers for the stellar.toml fetch and every SEP-0012 request.</param>
    /// <param name="cancellationToken">Cancellation token for the stellar.toml fetch.</param>
    /// <returns>A service bound to the anchor's KYC server.</returns>
    /// <exception cref="KycServiceException">
    ///     Thrown when the stellar.toml declares neither <c>KYC_SERVER</c> nor <c>TRANSFER_SERVER</c>, or declares one
    ///     that is not an acceptable service address: an absolute <c>https</c> URL (even for a loopback host) without
    ///     a query, fragment or user info.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="domain" /> is empty.</exception>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml cannot be fetched or parsed.</exception>
    public static async Task<KycService> FromDomainAsync(
        string domain,
        HttpResilienceOptions? resilienceOptions = null,
        string? bearerToken = null,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var toml = await StellarToml
            .FromDomainAsync(domain, resilienceOptions, bearerToken, httpClient, httpRequestHeaders, cancellationToken)
            .ConfigureAwait(false);

        var address = !string.IsNullOrWhiteSpace(toml.GeneralInformation.KycServer)
            ? toml.GeneralInformation.KycServer
            : toml.GeneralInformation.TransferServer;
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new KycServiceException(
                $"Neither KYC_SERVER nor TRANSFER_SERVER is declared in the stellar.toml of domain {domain}.");
        }

        // The address is anchor-controlled data, so a bad one is the anchor's error, not the caller's argument. A
        // discovered server must use https even on a loopback address: a remote stellar.toml must not be able to
        // point the JWT and customer data at a plain-http service on the wallet's own machine.
        if (!TryParseHttpUrl(address, out _, out var discovered) || discovered.Scheme != Uri.UriSchemeHttps)
        {
            throw new KycServiceException(
                $"The KYC server declared in the stellar.toml of domain {domain} is not an absolute https URL.");
        }

        try
        {
            return new KycService(address!, httpClient, resilienceOptions, httpRequestHeaders);
        }
        catch (ArgumentException ex)
        {
            throw new KycServiceException(
                $"The KYC server declared in the stellar.toml of domain {domain} is not usable: {ex.Message}",
                innerException: ex);
        }
    }

    /// <summary>
    ///     <c>GET /customer</c>: fetches the fields the anchor requires to register a customer, or the KYC status of a
    ///     customer that may already be registered.
    /// </summary>
    /// <param name="request">The request parameters and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer's status, the fields still needed, and the fields already provided.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT is empty or is not printable ASCII, <c>TransactionId</c> is set without <c>Type</c>, a
    ///     memo is given for a <c>C...</c> account, or <c>MemoType</c> is not <c>text</c>, <c>id</c> or <c>hash</c>.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when the JWT is rejected.</exception>
    /// <exception cref="CustomerNotFoundException">Thrown when the customer <c>id</c> is unknown (HTTP 404).</exception>
    /// <exception cref="InvalidKycResponseException">Thrown when the response body is not a valid SEP-0012 response.</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    public async Task<GetCustomerInfoResponse> GetCustomerInfoAsync(
        GetCustomerInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);
        ValidateIdentification(request.Account, request.Memo, request.MemoType);
        ValidateTransactionType(request.TransactionId, request.Type);

        var query = new List<KeyValuePair<string, string>>();
        AddIfPresent(query, "id", request.Id);
        AddIfPresent(query, "account", request.Account);
        AddIfPresent(query, "memo", request.Memo);
        AddIfPresent(query, "memo_type", request.MemoType);
        AddIfPresent(query, "type", request.Type);
        AddIfPresent(query, "transaction_id", request.TransactionId);
        AddIfPresent(query, "lang", request.Lang);

        var (body, status) = await SendAsync(HttpMethod.Get, BuildUrl("customer", query), null, request.Jwt,
            customerEndpoint: true, readSuccessBody: true, cancellationToken).ConfigureAwait(false);
        return ParseCustomerInfo(body, status);
    }

    /// <summary>
    ///     <c>PUT /customer</c>: idempotently uploads customer information — SEP-0009 fields, custom fields, binary
    ///     files, verification codes and references to previously uploaded files — as <c>multipart/form-data</c>, with
    ///     every binary part after all text parts.
    /// </summary>
    /// <param name="request">The customer data and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the created or updated customer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT is empty or is not printable ASCII, a field value is <c>null</c>, a field name is empty
    ///     or is not printable ASCII free of quotes and backslashes, a verification or file-reference key is only its
    ///     suffix, a name is used by both a text field and a file, <c>TransactionId</c> is set without <c>Type</c>, a
    ///     memo is given for a <c>C...</c> account, or <c>MemoType</c> is not <c>text</c>, <c>id</c> or <c>hash</c>.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when the JWT is rejected.</exception>
    /// <exception cref="CustomerNotFoundException">
    ///     Thrown when the customer <c>id</c> is unknown or belongs to another account (HTTP 404).
    /// </exception>
    /// <exception cref="PayloadTooLargeException">Thrown when an uploaded file exceeds the server's limit (HTTP 413).</exception>
    /// <exception cref="InvalidKycResponseException">Thrown when the response body is not a valid SEP-0012 response.</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    public async Task<PutCustomerInfoResponse> PutCustomerInfoAsync(
        PutCustomerInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);

        var fields = new Dictionary<string, string>();
        AddIfPresent(fields, "id", request.Id);
        AddIfPresent(fields, "account", request.Account);
        AddIfPresent(fields, "memo", request.Memo);
        AddIfPresent(fields, "memo_type", request.MemoType);
        AddIfPresent(fields, "type", request.Type);
        AddIfPresent(fields, "transaction_id", request.TransactionId);

        var files = new Dictionary<string, byte[]>();
        if (request.KycFields != null)
        {
            CopyInto(fields, request.KycFields.GetFields());
            if (request.KycFields.NaturalPerson != null)
            {
                CopyInto(files, request.KycFields.NaturalPerson.GetFiles());
            }

            if (request.KycFields.Organization != null)
            {
                CopyInto(files, request.KycFields.Organization.GetFiles());
            }
        }

        CopyInto(fields, request.CustomFields);
        CopyWithSuffix(fields, request.VerificationFields, VerificationSuffix);
        CopyWithSuffix(fields, request.FileReferences, FileIdSuffix);
        CopyInto(files, request.CustomFiles);

        // Text and binary parts are merged separately, so the later-source rule cannot pick between them: sending
        // both would leave the winner to the server, and a file part named "account" would bypass the checks below.
        foreach (var name in files.Keys)
        {
            if (fields.ContainsKey(name))
            {
                throw new ArgumentException(
                    $"Form field {UntrustedJsonValue.Describe(name)} is set both as a text field and as a file.",
                    nameof(request));
            }
        }

        // Checked on the merged fields, since CustomFields may set the same keys as the typed properties.
        ValidateIdentification(Get(fields, "account"), Get(fields, "memo"), Get(fields, "memo_type"));
        ValidateTransactionType(Get(fields, "transaction_id"), Get(fields, "type"));

        var content = BuildMultipart(fields, files);
        var (body, status) = await SendAsync(HttpMethod.Put, BuildUrl("customer"), content, request.Jwt,
            customerEndpoint: true, readSuccessBody: true, cancellationToken).ConfigureAwait(false);
        return Parse<PutCustomerInfoResponse>(body, status);
    }

    /// <summary>
    ///     <c>PUT /customer/verification</c> (<b>deprecated</b> since SEP-0012 v1.12.0): submits verification values,
    ///     such as confirmation codes, for fields in the <c>VERIFICATION_REQUIRED</c> status.
    /// </summary>
    /// <param name="request">The customer ID, verification values and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer's updated status, in the <c>GET /customer</c> response format.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT or ID is empty, the JWT is not printable ASCII, no verification value is given, or a
    ///     field name is invalid.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when the JWT is rejected.</exception>
    /// <exception cref="CustomerNotFoundException">Thrown when the customer <c>id</c> is unknown (HTTP 404).</exception>
    /// <exception cref="InvalidKycResponseException">Thrown when the response body is not a valid SEP-0012 response.</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    [Obsolete("PUT /customer/verification is deprecated since SEP-0012 v1.12.0. Use PutCustomerInfoAsync with " +
              "PutCustomerInfoRequest.VerificationFields instead.")]
    public async Task<GetCustomerInfoResponse> PutCustomerVerificationAsync(
        PutCustomerVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            throw new ArgumentException("The customer ID must not be empty.", nameof(request));
        }

        if (request.VerificationFields == null || request.VerificationFields.Count == 0)
        {
            throw new ArgumentException("At least one verification field is required.", nameof(request));
        }

        var fields = new Dictionary<string, string> { ["id"] = request.Id };
        CopyWithSuffix(fields, request.VerificationFields, VerificationSuffix);

        var content = BuildMultipart(fields, null);
        var (body, status) = await SendAsync(HttpMethod.Put, BuildUrl("customer/verification"), content,
            request.Jwt, customerEndpoint: true, readSuccessBody: true, cancellationToken).ConfigureAwait(false);
        return ParseCustomerInfo(body, status);
    }

    /// <summary>
    ///     <c>PUT /customer/callback</c>: registers the URL the anchor POSTs to whenever the customer's status changes,
    ///     replacing any previously registered URL. The response body is not read.
    /// </summary>
    /// <param name="request">The callback URL, customer identification and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT is empty or is not printable ASCII, the URL is not an absolute http(s) URL or uses
    ///     <c>http</c> for a host other than <c>localhost</c> or a standard-form loopback IP address, a memo is given for a <c>C...</c>
    ///     account, or <c>MemoType</c> is not <c>text</c>, <c>id</c> or <c>hash</c>.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when the JWT is rejected.</exception>
    /// <exception cref="CustomerNotFoundException">Thrown when the anchor has no information on the customer (HTTP 404).</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    public async Task PutCustomerCallbackAsync(
        PutCustomerCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);

        if (!TryParseHttpUrl(request.Url, out var url, out var callbackUri))
        {
            throw new ArgumentException("The callback URL must be an absolute http(s) URL.", nameof(request));
        }

        // The anchor POSTs a full GET /customer body to this URL; the Signature header authenticates it but does
        // not encrypt it, so the constructor's https rule applies here too.
        if (callbackUri.Scheme == Uri.UriSchemeHttp && !IsLoopbackHost(callbackUri, url))
        {
            throw new ArgumentException(
                "The callback URL must use https: the anchor posts customer data to it. " +
                "Plain http is accepted only for localhost or a loopback IP address in its standard form, " +
                "such as 127.0.0.1 or [::1].",
                nameof(request));
        }

        ValidateIdentification(request.Account, request.Memo, request.MemoType);

        var fields = new Dictionary<string, string> { ["url"] = url };
        AddIfPresent(fields, "id", request.Id);
        AddIfPresent(fields, "account", request.Account);
        AddIfPresent(fields, "memo", request.Memo);
        AddIfPresent(fields, "memo_type", request.MemoType);

        var content = BuildMultipart(fields, null);
        await SendAsync(HttpMethod.Put, BuildUrl("customer/callback"), content, request.Jwt,
            customerEndpoint: true, readSuccessBody: false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     <c>DELETE /customer/[account]</c>: deletes all personal information the anchor stores about the customer.
    ///     The memo parameters, when set, are sent in a <c>multipart/form-data</c> body. The response body is not read.
    /// </summary>
    /// <param name="request">The account, optional memo and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT is empty or is not printable ASCII, the account is empty or is a <c>.</c> or <c>..</c>
    ///     path segment, a memo is given for a <c>C...</c> account, or <c>MemoType</c> is not <c>text</c>, <c>id</c>
    ///     or <c>hash</c>.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">
    ///     Thrown when the JWT is rejected (HTTP 401, or 403 with type <c>authentication_required</c>).
    /// </exception>
    /// <exception cref="CustomerNotFoundException">Thrown when the anchor has no information on the customer (HTTP 404).</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    public async Task DeleteCustomerAsync(DeleteCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);

        if (string.IsNullOrWhiteSpace(request.Account))
        {
            throw new ArgumentException("The account must not be empty.", nameof(request));
        }

        // Uri.EscapeDataString leaves '.' alone, and System.Uri then collapses a "." or ".." segment, which would
        // send the DELETE to the customer collection or to the service root instead.
        if (request.Account == "." || request.Account == "..")
        {
            throw new ArgumentException("The account must not be a '.' or '..' path segment.", nameof(request));
        }

        ValidateIdentification(request.Account, request.Memo, request.MemoType);

        var fields = new Dictionary<string, string>();
        AddIfPresent(fields, "memo", request.Memo);
        AddIfPresent(fields, "memo_type", request.MemoType);

        var content = fields.Count > 0 ? BuildMultipart(fields, null) : null;
        await SendAsync(HttpMethod.Delete, BuildUrl("customer/" + Uri.EscapeDataString(request.Account)), content,
            request.Jwt, customerEndpoint: true, readSuccessBody: false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     <c>POST /customer/files</c>: uploads a single binary file, whose returned <c>file_id</c> can then be
    ///     referenced from <c>PUT /customer</c> through <see cref="PutCustomerInfoRequest.FileReferences" />.
    /// </summary>
    /// <param name="request">The file content, optional file name and content type, and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded file's metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT is empty or is not printable ASCII, the file is <c>null</c>, or the content type is not a
    ///     valid media type.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when the JWT is rejected.</exception>
    /// <exception cref="PayloadTooLargeException">Thrown when the file exceeds the server's limit (HTTP 413).</exception>
    /// <exception cref="InvalidKycResponseException">Thrown when the response body is not a valid SEP-0012 response.</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    public async Task<CustomerFileResponse> PostCustomerFileAsync(
        PostCustomerFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);

        if (request.File == null)
        {
            throw new ArgumentException("The file content must not be null.", nameof(request));
        }

        var fileName = string.IsNullOrWhiteSpace(request.FileName) ? "file" : EncodeFileName(request.FileName!);

        var filePart = new ByteArrayContent(request.File);
        filePart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = Quote("file"),
            FileName = Quote(fileName),
        };
        try
        {
            filePart.Headers.ContentType = MediaTypeHeaderValue.Parse(
                string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType!);
        }
        catch (FormatException ex)
        {
            filePart.Dispose();
            throw new ArgumentException("The file content type is not a valid media type.", nameof(request), ex);
        }

        var content = new MultipartFormDataContent { filePart };

        var (body, status) = await SendAsync(HttpMethod.Post, BuildUrl("customer/files"), content, request.Jwt,
            customerEndpoint: false, readSuccessBody: true, cancellationToken).ConfigureAwait(false);
        return Parse<CustomerFileResponse>(body, status);
    }

    /// <summary>
    ///     <c>GET /customer/files</c>: fetches metadata of files uploaded with <c>POST /customer/files</c>, by file ID
    ///     or by customer ID.
    /// </summary>
    /// <param name="request">The file or customer ID, and JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching files; an empty list when none match.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the JWT is empty or is not printable ASCII, or neither a file ID nor a customer ID is given.
    /// </exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when the JWT is rejected.</exception>
    /// <exception cref="InvalidKycResponseException">Thrown when the response body is not a valid SEP-0012 response.</exception>
    /// <exception cref="KycServiceException">Thrown for any other error response.</exception>
    public async Task<GetCustomerFilesResponse> GetCustomerFilesAsync(
        GetCustomerFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateJwt(request.Jwt);

        if (string.IsNullOrWhiteSpace(request.FileId) && string.IsNullOrWhiteSpace(request.CustomerId))
        {
            throw new ArgumentException("Either a file ID or a customer ID is required.", nameof(request));
        }

        var query = new List<KeyValuePair<string, string>>();
        AddIfPresent(query, "file_id", request.FileId);
        AddIfPresent(query, "customer_id", request.CustomerId);

        var (body, status) = await SendAsync(HttpMethod.Get, BuildUrl("customer/files", query), null, request.Jwt,
            customerEndpoint: false, readSuccessBody: true, cancellationToken).ConfigureAwait(false);
        return Parse<GetCustomerFilesResponse>(body, status);
    }

    private static HttpMessageHandler CreateNonRedirectingHandler()
    {
#if NET8_0_OR_GREATER
        return new SocketsHttpHandler { AllowAutoRedirect = false };
#else
        return new HttpClientHandler { AllowAutoRedirect = false };
#endif
    }

    private string BuildUrl(string path, List<KeyValuePair<string, string>>? query = null)
    {
        var url = $"{ServiceAddress}/{path}";
        if (query == null || query.Count == 0)
        {
            return url;
        }

        return url + "?" + string.Join("&",
            query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    /// <summary>
    ///     Sends a request and returns the success body with its status code, or throws the exception that the error
    ///     status maps to.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The absolute request URL.</param>
    /// <param name="content">The request body, or <c>null</c>; disposed with the request.</param>
    /// <param name="jwt">
    ///     The SEP-10 or SEP-45 JWT sent as the bearer token, already checked with <see cref="ValidateJwt" />.
    /// </param>
    /// <param name="customerEndpoint">
    ///     Whether the endpoint is a <c>/customer</c> endpoint, on which 404 means "customer not found".
    /// </param>
    /// <param name="readSuccessBody">
    ///     Whether the caller parses a success body. When it does not, a success response is returned without reading
    ///     its body, so a body the caller would discard cannot turn a completed operation into an error.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task<(string? Body, int Status)> SendAsync(
        HttpMethod method,
        string url,
        HttpContent? content,
        string jwt,
        bool customerEndpoint,
        bool readSuccessBody,
        CancellationToken cancellationToken)
    {
        // After every argument check, so a disposed service reports an invalid request the same way a live one does.
        if (_disposed)
        {
            content?.Dispose();
            throw new ObjectDisposedException(nameof(KycService));
        }

        HttpRequestMessage unownedRequest;
        try
        {
            unownedRequest = new HttpRequestMessage(method, url);
        }
        catch
        {
            // UriFormatException for an over-long URL; the request does not own the content yet.
            content?.Dispose();
            throw;
        }

        using var httpRequest = unownedRequest;
        httpRequest.Content = content;
        var requestUri = httpRequest.RequestUri!;
        if (_httpRequestHeaders != null)
        {
            foreach (var header in _httpRequestHeaders)
            {
                // A content header the service already set (the multipart Content-Type) is not overridden: adding
                // a second value would corrupt the boundary parameter the server needs to parse the body.
                if (!httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value) &&
                    httpRequest.Content != null && !httpRequest.Content.Headers.TryGetValues(header.Key, out _))
                {
                    httpRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Set after the custom headers so the JWT always wins over a caller-supplied Authorization header.
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // ResponseHeadersRead stops HttpClient from buffering the whole body before the size limit can apply. It
        // also takes the body out of HttpClient.Timeout's reach, so the same deadline is re-imposed here over the
        // whole exchange; without it a server that trickles its body would hold the call open indefinitely.
        var timeout = GetTimeout();
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout != Timeout.InfiniteTimeSpan)
        {
            timeoutSource.CancelAfter(timeout);
        }

        try
        {
            using var response = await _httpClient
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, timeoutSource.Token)
                .ConfigureAwait(false);
            var status = (int)response.StatusCode;

            // A client that follows redirects updates the request's URI to the final location.
            var finalUri = response.RequestMessage?.RequestUri ?? httpRequest.RequestUri ?? requestUri;
            if (Uri.Compare(finalUri, requestUri, UriComponents.SchemeAndServer, UriFormat.UriEscaped,
                    StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new KycServiceException(
                    $"SEP-0012 {method.Method} request was redirected to a different origin; the response was " +
                    "rejected.", status);
            }

            // Following a redirect, HttpClient resends a POST as GET on 301-303 and a PUT or DELETE as GET on 303.
            // The final response then answers a GET the caller never made, and may not mean the change was applied.
            var finalMethod = response.RequestMessage?.Method ?? method;
            if (finalMethod != method)
            {
                throw new KycServiceException(
                    $"SEP-0012 {method.Method} request was redirected and resent as {finalMethod.Method}; the " +
                    "response was rejected.", status);
            }

            if (response.IsSuccessStatusCode && !readSuccessBody)
            {
                return (null, status);
            }

            string? body;
            try
            {
                var encoding = response.IsSuccessStatusCode ? StrictUtf8 : Encoding.UTF8;
                body = await ReadBodyBoundedAsync(response, encoding, timeoutSource.Token).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                if (response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Error while reading the SEP-0012 response body.", ex);
                }

                // The status alone still selects the exception type; only the anchor's error text is lost.
                body = null;
            }
            catch (DecoderFallbackException ex)
            {
                // Only the strict decoder throws this, and only success bodies use it.
                throw new InvalidKycResponseException("SEP-0012 response body is not valid UTF-8.", status, ex);
            }

            if (response.IsSuccessStatusCode)
            {
                if (body == null)
                {
                    throw new InvalidKycResponseException(
                        $"SEP-0012 response body exceeds the {MaxResponseBodyBytes}-byte limit.", status);
                }

                return (body, status);
            }

            throw CreateErrorException(method, status, body, customerEndpoint,
                RetryAfterParser.ToTimeSpan(response.Headers.RetryAfter));
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested &&
                                                   timeoutSource.IsCancellationRequested)
        {
            throw CreateTimeoutException(timeout, ex);
        }
        catch (TimeoutRejectedException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A RetryingHttpMessageHandler's per-request timeout. Its duration is known only for the internal client;
            // an external client's handler can carry any value, unrelated to HttpClient.Timeout.
            throw CreateTimeoutException(_requestTimeout, ex);
        }
    }

    /// <summary>
    ///     The deadline for one whole exchange: the client's <see cref="HttpClient.Timeout" />, tightened by the
    ///     internal client's <see cref="HttpResilienceOptions.RequestTimeout" />. Read on every call rather than cached,
    ///     so it always matches the client's own setting.
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

    /// <summary>
    ///     Builds the exception for a timed-out request, naming the duration only when <paramref name="timeout" /> is
    ///     known.
    /// </summary>
    private static TaskCanceledException CreateTimeoutException(TimeSpan? timeout, Exception cause)
    {
        var message = timeout is { } known
            ? "The SEP-0012 request was canceled due to the configured timeout of " +
              $"{known.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds elapsing."
            : "The SEP-0012 request was canceled due to a timeout in the HTTP client's message handler.";
        return new TaskCanceledException(message, new TimeoutException(cause.Message, cause));
    }

    /// <summary>
    ///     Reads a response body into a string, or returns <c>null</c> when it exceeds
    ///     <see cref="MaxResponseBodyBytes" />. A declared <c>Content-Length</c> over the limit is refused before
    ///     anything is read; an undeclared or understated length is caught while streaming. A leading UTF-8 byte order
    ///     mark is skipped, as <see cref="HttpContent.ReadAsStringAsync()" /> would.
    /// </summary>
    /// <exception cref="DecoderFallbackException">
    ///     Thrown when <paramref name="encoding" /> throws on invalid input and the body is not valid UTF-8.
    /// </exception>
    private static async Task<string?> ReadBodyBoundedAsync(HttpResponseMessage response, Encoding encoding,
        CancellationToken cancellationToken)
    {
        // Never null on .NET 5+, but netstandard2.1 also runs on older runtimes where it can be.
        if (response.Content == null)
        {
            return string.Empty;
        }

        if (response.Content.Headers.ContentLength is > MaxResponseBodyBytes)
        {
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > MaxResponseBodyBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        var bytes = buffer.GetBuffer();
        var length = (int)buffer.Length;
        var offset = length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        return encoding.GetString(bytes, offset, length - offset);
    }

    private static KycServiceException CreateErrorException(HttpMethod method, int status, string? body,
        bool customerEndpoint, TimeSpan? retryAfterDelay)
    {
        KycErrorResponse? error = null;
        Exception? parseFailure = null;
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                error = JsonSerializer.Deserialize<KycErrorResponse>(body!, JsonOptions.DefaultOptions);
            }
            catch (JsonException ex)
            {
                // Not a SEP-0012 error object (HTML error page, duplicate keys, non-string "error", ...). The status
                // code alone still selects the exception type; the parse failure is kept, sanitized, as the inner
                // exception.
                parseFailure = UntrustedText.Sanitize(ex);
            }
        }

        var errorText = error?.Error;
        var message = $"SEP-0012 {method.Method} request failed with HTTP {status}" +
                      (errorText != null ? $": {UntrustedText.Sanitize(errorText)}" : ".");

        if (status == 401 || (status == 403 && error?.Type == AuthenticationRequiredType))
        {
            return new AuthenticationRequiredException(message, status, errorText, parseFailure);
        }

        if (status == 404 && customerEndpoint)
        {
            return new CustomerNotFoundException(message, errorText, parseFailure);
        }

        if (status == 413)
        {
            return new PayloadTooLargeException(message, errorText, parseFailure);
        }

        return new KycServiceException(message, status, errorText, parseFailure, retryAfterDelay);
    }

    private static GetCustomerInfoResponse ParseCustomerInfo(string? body, int status)
    {
        return Parse<GetCustomerInfoResponse>(body, status);
    }

    private static T Parse<T>(string? body, int status) where T : class
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidKycResponseException("SEP-0012 response contains no content.", status);
        }

        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(body!, JsonOptions.DefaultOptions);
        }
        catch (JsonException ex)
        {
            // System.Text.Json's message ends with the JSON path, which quotes server-chosen dictionary keys.
            var sanitized = UntrustedText.Sanitize(ex);
            throw new InvalidKycResponseException($"Invalid SEP-0012 {typeof(T).Name}: {sanitized.Message}",
                status, sanitized);
        }

        return result ?? throw new InvalidKycResponseException(
            $"SEP-0012 {typeof(T).Name} is the JSON literal null.", status);
    }

    private static MultipartFormDataContent BuildMultipart(
        Dictionary<string, string> fields,
        Dictionary<string, byte[]>? files)
    {
        // Validate every name before building anything, so an invalid one does not leave parts undisposed.
        foreach (var field in fields)
        {
            ValidatePartName(field.Key);
            if (field.Value == null)
            {
                throw new ArgumentException(
                    $"The value of form field {UntrustedJsonValue.Describe(field.Key)} must not be null.");
            }
        }

        if (files != null)
        {
            foreach (var file in files)
            {
                ValidatePartName(file.Key);
                if (file.Value == null)
                {
                    throw new ArgumentException(
                        $"The content of file field {UntrustedJsonValue.Describe(file.Key)} must not be null.");
                }
            }
        }

        var content = new MultipartFormDataContent();
        foreach (var field in fields)
        {
            var part = new StringContent(field.Value, Encoding.UTF8);
            // Text parts carry no Content-Type, like a browser form submission.
            part.Headers.ContentType = null;
            part.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = Quote(field.Key),
            };
            content.Add(part);
        }

        // SEP-0012: binary fields go after all other fields, so servers can stream them.
        if (files != null)
        {
            foreach (var file in files)
            {
                var part = new ByteArrayContent(file.Value);
                part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                part.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = Quote(file.Key),
                    FileName = Quote(file.Key),
                };
                content.Add(part);
            }
        }

        return content;
    }

    /// <summary>
    ///     Rejects a form-data part name that would not reach the server intact. A quote or backslash ends or escapes
    ///     the quoted-string early, CR/LF would inject further headers into the part, and a non-ASCII character makes
    ///     <see cref="ContentDispositionHeaderValue" /> emit an RFC 2047 encoded-word (<c>=?utf-8?B?...?=</c>), which
    ///     <c>multipart/form-data</c> servers do not decode (RFC 7578, section 5.1). SEP-0009 and anchor field names
    ///     are plain ASCII identifiers.
    /// </summary>
    private static void ValidatePartName(string name)
    {
        var valid = !string.IsNullOrEmpty(name);
        if (valid)
        {
            foreach (var c in name)
            {
                if (c < 0x20 || c > 0x7E || c == '"' || c == '\\')
                {
                    valid = false;
                    break;
                }
            }
        }

        if (!valid)
        {
            throw new ArgumentException(
                $"Invalid form field name {UntrustedJsonValue.Describe(name)}: it must be non-empty printable ASCII " +
                "without a quote or backslash.");
        }
    }

    /// <summary>
    ///     Percent-encodes a file name for the <c>filename</c> parameter, as RFC 7578 (section 4.2) allows for
    ///     multipart/form-data: every byte of its UTF-8 form outside printable ASCII, and the quote, backslash and
    ///     percent sign, becomes <c>%XX</c>. <see cref="ContentDispositionHeaderValue" /> would otherwise encode a
    ///     non-ASCII name as an RFC 2047 encoded-word, which servers store verbatim.
    /// </summary>
    private static string EncodeFileName(string fileName)
    {
        var builder = new StringBuilder(fileName.Length);
        foreach (var b in Encoding.UTF8.GetBytes(fileName))
        {
            if (b >= 0x20 && b <= 0x7E && b != (byte)'"' && b != (byte)'\\' && b != (byte)'%')
            {
                builder.Append((char)b);
            }
            else
            {
                builder.Append('%').Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Rejects a JWT that is empty or is not printable ASCII. Every public method calls it first, before any request
    ///     content exists, so an invalid JWT never leaves content to dispose.
    /// </summary>
    private static void ValidateJwt(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            throw new ArgumentException("A SEP-10 or SEP-45 JWT is required for every SEP-0012 request.",
                "request");
        }

        foreach (var c in jwt)
        {
            // A JWT is base64url segments joined by dots. Anything outside printable ASCII would otherwise surface
            // from System.Net.Http as a FormatException or an HttpRequestException that looks like a transport fault.
            if (c <= ' ' || c > '~')
            {
                throw new ArgumentException(
                    "The JWT must consist of printable ASCII characters, without whitespace.", "request");
            }
        }
    }

    /// <summary>
    ///     Enforces the SEP-0012 identification rules a client can check before sending: no memo for a <c>C...</c>
    ///     contract account, and a <c>memo_type</c> of <c>text</c>, <c>id</c> or <c>hash</c>.
    /// </summary>
    private static void ValidateIdentification(string? account, string? memo, string? memoType)
    {
        if (!string.IsNullOrWhiteSpace(memo) && !string.IsNullOrWhiteSpace(account) &&
            StrKey.IsValidContractId(account!))
        {
            throw new ArgumentException("SEP-0012 forbids a memo when the account is a C... contract account.",
                "request");
        }

        if (!string.IsNullOrWhiteSpace(memoType) && Array.IndexOf(MemoTypes, memoType) < 0)
        {
            throw new ArgumentException(
                $"Invalid memo type {UntrustedJsonValue.Describe(memoType)}: SEP-0012 allows text, id or hash.",
                "request");
        }
    }

    /// <summary>
    ///     SEP-0012: "If <c>transaction_id</c> is in use, <c>type</c> must be provided".
    /// </summary>
    private static void ValidateTransactionType(string? transactionId, string? type)
    {
        if (!string.IsNullOrWhiteSpace(transactionId) && string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("SEP-0012 requires a type whenever a transaction ID is given.",
                "request");
        }
    }

    /// <summary>
    ///     Parses an absolute http(s) URL, trimmed of surrounding whitespace, whose text continues with <c>://</c>
    ///     right after the scheme. <see cref="Uri" /> also accepts forms such as <c>https:\\host</c>, which other
    ///     parsers (the anchor's among them) read differently.
    /// </summary>
    internal static bool TryParseHttpUrl(string? text, out string trimmed, out Uri uri)
    {
        trimmed = text?.Trim() ?? string.Empty;
        uri = null!;
        if (trimmed.Length == 0 || !Uri.TryCreate(trimmed, UriKind.Absolute, out var parsed) ||
            (parsed.Scheme != Uri.UriSchemeHttps && parsed.Scheme != Uri.UriSchemeHttp) ||
            string.Compare(trimmed, parsed.Scheme.Length, "://", 0, 3, StringComparison.Ordinal) != 0)
        {
            return false;
        }

        uri = parsed;
        return true;
    }

    /// <summary>
    ///     Returns the authority of a URL accepted by <see cref="TryParseHttpUrl" /> exactly as it is written —
    ///     host case and an explicit port preserved, user info removed. <see cref="Uri" /> normalises the host (it
    ///     lower-cases it, and even rewrites the name <c>loopback</c> to <c>localhost</c>).
    /// </summary>
    internal static string GetAuthorityAsWritten(string trimmedUrl)
    {
        var start = trimmedUrl.IndexOf("://", StringComparison.Ordinal) + 3;
        var end = trimmedUrl.IndexOfAny(new[] { '/', '?', '#' }, start);
        var authority = end < 0 ? trimmedUrl.Substring(start) : trimmedUrl.Substring(start, end - start);
        return authority.Substring(authority.LastIndexOf('@') + 1);
    }

    /// <summary>
    ///     Whether a URL's host, as written, is the name <c>localhost</c> or a loopback IP address in its standard
    ///     form. <see cref="Uri.IsLoopback" /> is not used: it also accepts the name <c>loopback</c>, which DNS can
    ///     resolve anywhere, and its handling of an upper-case <c>LOCALHOST</c> differs between runtimes. An IP
    ///     address must be written as <see cref="Uri" /> prints it: <see cref="Uri" /> also reads <c>0177.0.0.1</c>,
    ///     <c>0x7f.1</c>, <c>127.1</c> and <c>2130706433</c> as <c>127.0.0.1</c>, while other parsers (such as the
    ///     anchor's, for a callback URL) may read them as a public address or a DNS name.
    /// </summary>
    private static bool IsLoopbackHost(Uri uri, string trimmedUrl)
    {
        var authority = GetAuthorityAsWritten(trimmedUrl);
        var hostEnd = authority.StartsWith("[", StringComparison.Ordinal)
            ? authority.IndexOf(']') + 1
            : authority.LastIndexOf(':');
        var host = hostEnd <= 0 ? authority : authority.Substring(0, hostEnd);

        // Uri reads anything after an IPv6 literal's ']' other than a port ("[::1]x") as part of the path.
        if (hostEnd > 0 && hostEnd < authority.Length && authority[hostEnd] != ':')
        {
            return false;
        }

        // Both branches require the host as written to equal Uri.Host, so the two parses must agree on where the
        // host ends. Uri.Host is already lower-cased, so LOCALHOST still passes.
        if (uri.HostNameType == UriHostNameType.Dns)
        {
            return string.Equals(host, uri.Host, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(host, uri.Host, StringComparison.OrdinalIgnoreCase) &&
               IPAddress.TryParse(uri.DnsSafeHost, out var address) && IPAddress.IsLoopback(address);
    }

    private static string? Get(Dictionary<string, string> fields, string key)
    {
        return fields.TryGetValue(key, out var value) ? value : null;
    }

    private static string Quote(string value)
    {
        return "\"" + value + "\"";
    }

    private static void AddIfPresent(List<KeyValuePair<string, string>> target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target.Add(new KeyValuePair<string, string>(key, value!));
        }
    }

    private static void AddIfPresent(Dictionary<string, string> target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target[key] = value!;
        }
    }

    private static void CopyInto<T>(Dictionary<string, T> target, IReadOnlyDictionary<string, T>? source)
    {
        if (source == null)
        {
            return;
        }

        foreach (var entry in source)
        {
            target[entry.Key] = entry.Value;
        }
    }

    private static void CopyWithSuffix(Dictionary<string, string> target,
        IReadOnlyDictionary<string, string>? source, string suffix)
    {
        if (source == null)
        {
            return;
        }

        foreach (var entry in source)
        {
            // Validate the caller's key before suffixing it: an empty key, or a key that is only the suffix, would
            // otherwise become a part named just "_verification" or "_file_id" and pass the part-name check.
            ValidatePartName(entry.Key);
            if (entry.Key == suffix)
            {
                throw new ArgumentException(
                    $"Invalid form field name {UntrustedJsonValue.Describe(entry.Key)}: it names no field, only the " +
                    $"'{suffix}' suffix.");
            }

            var key = entry.Key.EndsWith(suffix, StringComparison.Ordinal) ? entry.Key : entry.Key + suffix;
            target[key] = entry.Value;
        }
    }
}
