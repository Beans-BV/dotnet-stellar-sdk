using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0010.Exceptions;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Sep.Sep0010;

/// <summary>
///     Implements the SEP-0010 Web Authentication protocol for Stellar applications.
///     This class implements SEP-0010 version 3.4.1, which defines a standard protocol
///     for authenticating users of Stellar applications using their Stellar account.
/// </summary>
/// <remarks>
///     <para>
///         <b>Typical authentication flow</b>
///     </para>
///     <para>
///         A typical SEP-0010 authentication flow using this class is:
///     </para>
///     <list type="number">
///         <item>
///             <description>
///                 Create a <see cref="WebAuth"/> instance. In most cases you should use
///                 <see cref="FromDomainAsync(string, HttpClient?, Dictionary{string, string}?, int?)"/> to load
///                 configuration from the anchor&apos;s <c>stellar.toml</c> (WEB_AUTH_ENDPOINT, SIGNING_KEY, and network).
///                 Use the constructor directly only when you already know all parameters or need custom configuration.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Call <c>BuildChallengeTransactionAsync</c> (or the appropriate method) to fetch or construct a
///                 challenge transaction from the SEP-0010 server for the user&apos;s Stellar account.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Have the user sign the challenge transaction with their Stellar keypair (typically on-device or in a wallet).
///             </description>
///         </item>
///         <item>
///             <description>
///                 Submit the signed challenge back to the server using the corresponding method on this class.
///                 If the server verifies the signatures and validity window, it issues a JWT or other auth token
///                 according to SEP-0010.
///             </description>
///         </item>
///     </list>
///     <para>
///         <b>Choosing between constructor and FromDomainAsync</b>
///     </para>
///     <para>
///         Use <see cref="FromDomainAsync(string, HttpClient?, Dictionary{string, string}?, int?)"/> when you have a
///         Stellar home domain (for example, <c>"example.com"</c>) and want the SDK to discover the WEB_AUTH_ENDPOINT,
///         SIGNING_KEY, and network passphrase from the domain&apos;s <c>stellar.toml</c>. This is the recommended,
///         high-level entry point for most clients.
///     </para>
///     <para>
///         Use the public constructor when you already know the auth endpoint URL, network, and server signing key
///         (for example, in test environments or when configuration is not coming from <c>stellar.toml</c>), or when
///         you need to inject additional headers or other advanced settings explicitly.
///     </para>
///     <para>
///         <b>IDisposable and HttpClient ownership</b>
///     </para>
///     <para>
///         <see cref="WebAuth"/> implements <see cref="IDisposable"/> because it may create and own an internal
///         <see cref="HttpClient"/> instance. When you let <see cref="WebAuth"/> create its own client (for example,
///         by calling <see cref="FromDomainAsync(string, HttpClient?, Dictionary{string, string}?, int?)"/> without
///         passing an <see cref="HttpClient"/>), you should either wrap <see cref="WebAuth"/> in a <c>using</c> block
///         or explicitly call <see cref="Dispose"/> when you are finished with it so the internal client is cleaned up.
///     </para>
///     <para>
///         If you pass an external <see cref="HttpClient"/> into the constructor or <see cref="FromDomainAsync(string, HttpClient?, Dictionary{string, string}?, int?)"/>,
///         that client remains owned by the caller. In that case, disposing <see cref="WebAuth"/> will not dispose the
///         external client, and you are responsible for managing the <see cref="HttpClient"/> lifecycle yourself
///         (for example, by reusing a single long-lived instance for performance and resilience).
///     </para>
/// </remarks>
public class WebAuth : IDisposable
{
    private const string WebAuthDataKey = "web_auth_domain";
    private const string ClientDomainDataKey = "client_domain";
    private const string AuthSuffix = " auth";

    /// <summary>
    ///     Give a small grace period for the transaction time to account for clock drift.
    /// </summary>
    public const int GracePeriod = 60 * 5; // 5 minutes

    private readonly string _authEndpoint;
    private readonly Network _network;
    private readonly string _serverSigningKey;
    private readonly string _serverHomeDomain;
    private readonly HttpClient _httpClient;
    private readonly bool _internalHttpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly int _gracePeriod;

    /// <summary>
    ///     Creates a WebAuth instance with explicit configuration.
    /// </summary>
    /// <param name="authEndpoint">The authentication endpoint URL (from stellar.toml WEB_AUTH_ENDPOINT)</param>
    /// <param name="network">The Stellar network (Network.Public() or Network.Test())</param>
    /// <param name="serverSigningKey">The server's public signing key (from stellar.toml SIGNING_KEY)</param>
    /// <param name="serverHomeDomain">The home domain of the server</param>
    /// <param name="httpClient">Optional custom HTTP client for testing or proxy configuration</param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers for all requests</param>
    /// <param name="gracePeriod">Optional grace period in seconds for time bounds validation (default: 300)</param>
    public WebAuth(
        string authEndpoint,
        Network network,
        string serverSigningKey,
        string serverHomeDomain,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        int? gracePeriod = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(authEndpoint);
        ArgumentNullException.ThrowIfNull(network);
        ArgumentException.ThrowIfNullOrEmpty(serverSigningKey);
        ArgumentException.ThrowIfNullOrEmpty(serverHomeDomain);

        _authEndpoint = authEndpoint;
        _network = network;
        _serverSigningKey = serverSigningKey;
        _serverHomeDomain = serverHomeDomain;
        _httpRequestHeaders = httpRequestHeaders;
        _gracePeriod = gracePeriod ?? GracePeriod;

        if (httpClient != null)
        {
            _httpClient = httpClient;
            _internalHttpClient = false;
        }
        else
        {
            _httpClient = new DefaultStellarSdkHttpClient();
            _internalHttpClient = true;
        }
    }

    /// <summary>
    ///     Creates a WebAuth instance by automatically discovering configuration from stellar.toml.
    /// </summary>
    /// <param name="domain">The domain name (without protocol) hosting the stellar.toml file</param>
    /// <param name="network">The Stellar network (Network.Public() or Network.Test())</param>
    /// <param name="httpClient">Optional custom HTTP client for testing or proxy configuration</param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers for requests</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests</param>
    /// <param name="bearerToken">Optional bearer token for stellar.toml requests</param>
    /// <returns>WebAuth instance configured with the domain's settings</returns>
    public static async Task<WebAuth> FromDomainAsync(
        string domain,
        Network network,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        HttpResilienceOptions? resilienceOptions = null,
        string? bearerToken = null)
    {
        var toml = await StellarToml.FromDomainAsync(domain, resilienceOptions, bearerToken, httpClient, httpRequestHeaders)
            .ConfigureAwait(false);

        if (toml.GeneralInformation.WebAuthEndpoint == null)
        {
            throw new NoWebAuthEndpointFoundException(domain);
        }

        if (toml.GeneralInformation.SigningKey == null)
        {
            throw new NoWebAuthServerSigningKeyFoundException(domain);
        }

        return new WebAuth(
            toml.GeneralInformation.WebAuthEndpoint,
            network,
            toml.GeneralInformation.SigningKey,
            domain,
            httpClient,
            httpRequestHeaders);
    }

    /// <summary>
    ///     Performs the complete SEP-0010 authentication flow and returns a JWT token.
    /// </summary>
    /// <param name="clientAccountId">The Stellar account ID to authenticate (G... or M... address)</param>
    /// <param name="signers">List of keypairs needed to sign for the account</param>
    /// <param name="memo">Optional ID memo if using a muxed account that starts with G</param>
    /// <param name="homeDomain">Optional home domain if the auth server serves multiple domains</param>
    /// <param name="clientDomain">Optional domain of the client application</param>
    /// <param name="clientDomainAccountKeyPair">Optional keypair for the client domain's signing key</param>
    /// <param name="clientDomainSigningDelegate">Optional async callback to sign the challenge with the client domain key</param>
    /// <returns>The JWT authentication token</returns>
    public async Task<string> JwtTokenAsync(
        string clientAccountId,
        ICollection<KeyPair> signers,
        int? memo = null,
        string? homeDomain = null,
        string? clientDomain = null,
        KeyPair? clientDomainAccountKeyPair = null,
        ClientDomainSigningDelegate? clientDomainSigningDelegate = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientAccountId);
        ArgumentNullException.ThrowIfNull(signers);

        if (memo != null && clientAccountId.StartsWith("M", StringComparison.Ordinal))
        {
            throw new NoMemoForMuxedAccountsException();
        }

        // Get the challenge transaction from the web auth server
        var challengeTransaction = await GetChallengeAsync(clientAccountId, memo, homeDomain, clientDomain)
            .ConfigureAwait(false);

        string? clientDomainAccountId = null;
        if (clientDomainAccountKeyPair != null)
        {
            clientDomainAccountId = clientDomainAccountKeyPair.AccountId;
        }
        else if (clientDomainSigningDelegate != null)
        {
            if (clientDomain == null)
            {
                throw new MissingClientDomainException();
            }

            var clientToml = await StellarToml.FromDomainAsync(
                    clientDomain,
                    null,
                    null,
                    _httpClient,
                    _httpRequestHeaders)
                .ConfigureAwait(false);

            if (clientToml.GeneralInformation.SigningKey == null)
            {
                throw new NoClientDomainSigningKeyFoundException(clientDomain);
            }

            clientDomainAccountId = clientToml.GeneralInformation.SigningKey;
        }

        // Validate the transaction received from the web auth server
        ValidateChallenge(challengeTransaction, clientAccountId, clientDomainAccountId, _gracePeriod, memo);

        // Sign with client domain key if needed
        if (clientDomainAccountKeyPair != null)
        {
            challengeTransaction = SignTransaction(challengeTransaction, new[] { clientDomainAccountKeyPair });
        }
        else if (clientDomainSigningDelegate != null)
        {
            challengeTransaction = await clientDomainSigningDelegate(challengeTransaction).ConfigureAwait(false);
        }

        // Sign the transaction with the provided user/client keypairs
        var signedTransaction = SignTransaction(challengeTransaction, signers);

        // Request the jwt token by sending back the signed challenge transaction
        var jwtToken = await SendSignedChallengeAsync(signedTransaction).ConfigureAwait(false);

        return jwtToken;
    }

    /// <summary>
    ///     Requests a challenge transaction from the WebAuth server.
    /// </summary>
    /// <param name="clientAccountId">The Stellar account ID requesting authentication</param>
    /// <param name="memo">Optional ID memo for G... addresses (not allowed for M... addresses)</param>
    /// <param name="homeDomain">Optional home domain if server serves multiple domains</param>
    /// <param name="clientDomain">Optional client application domain for domain verification</param>
    /// <returns>The base64-encoded XDR transaction envelope</returns>
    public async Task<string> GetChallengeAsync(
        string clientAccountId,
        int? memo = null,
        string? homeDomain = null,
        string? clientDomain = null)
    {
        var challengeResponse = await GetChallengeResponseAsync(clientAccountId, memo, homeDomain, clientDomain)
            .ConfigureAwait(false);

        if (challengeResponse.Transaction == null)
        {
            throw new MissingTransactionInChallengeResponseException();
        }

        return challengeResponse.Transaction;
    }

    /// <summary>
    ///     Requests a challenge response from the WebAuth server.
    /// </summary>
    /// <param name="accountId">The Stellar account ID requesting authentication</param>
    /// <param name="memo">Optional ID memo for G... addresses</param>
    /// <param name="homeDomain">Optional home domain if server serves multiple domains</param>
    /// <param name="clientDomain">Optional client application domain</param>
    /// <returns>ChallengeResponse with transaction XDR and optional network_passphrase</returns>
    public async Task<ChallengeResponse> GetChallengeResponseAsync(
        string accountId,
        int? memo = null,
        string? homeDomain = null,
        string? clientDomain = null)
    {
        if (memo != null && accountId.StartsWith("M", StringComparison.Ordinal))
        {
            throw new NoMemoForMuxedAccountsException();
        }

        var uriBuilder = new UriBuilder(_authEndpoint);
        var queryParams = new List<string> { $"account={Uri.EscapeDataString(accountId)}" };

        if (homeDomain != null)
        {
            queryParams.Add($"home_domain={Uri.EscapeDataString(homeDomain)}");
        }

        if (clientDomain != null)
        {
            queryParams.Add($"client_domain={Uri.EscapeDataString(clientDomain)}");
        }

        if (memo != null)
        {
            queryParams.Add($"memo={memo}");
        }

        uriBuilder.Query = string.Join("&", queryParams);
        var requestUri = uriBuilder.Uri;

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (_httpRequestHeaders != null)
        {
            foreach (var header in _httpRequestHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if ((int)response.StatusCode >= 300)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new ChallengeRequestErrorException((int)response.StatusCode, errorBody);
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var challengeResponse = JsonSerializer.Deserialize<ChallengeResponse>(content, JsonOptions.DefaultOptions);

            if (challengeResponse == null)
            {
                throw new ChallengeRequestErrorException(500, "Failed to deserialize challenge response");
            }

            return challengeResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new ChallengeRequestErrorException(0, ex.Message);
        }
    }

    /// <summary>
    ///     Validates a challenge transaction according to SEP-0010 requirements.
    /// </summary>
    /// <param name="challengeTransaction">Base64-encoded XDR transaction envelope to validate</param>
    /// <param name="userAccountId">The user's account ID that requested the challenge</param>
    /// <param name="clientDomainAccountId">Optional client domain account ID if domain verification is used</param>
    /// <param name="timeBoundsGracePeriod">Optional grace period in seconds for time bounds validation</param>
    /// <param name="memo">Optional expected memo value for muxed accounts</param>
    public void ValidateChallenge(
        string challengeTransaction,
        string userAccountId,
        string? clientDomainAccountId = null,
        int? timeBoundsGracePeriod = null,
        int? memo = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(challengeTransaction);
        ArgumentException.ThrowIfNullOrEmpty(userAccountId);

        // Convert to SDK Transaction early - single conversion point
        var transaction = Transactions.Transaction.FromEnvelopeXdr(challengeTransaction);

        // Validate sequence number
        if (transaction.SequenceNumber != 0)
        {
            throw new ChallengeValidationErrorInvalidSeqNr("Invalid transaction, sequence number not 0");
        }

        // Validate memo using SDK types
        if (transaction.Memo is MemoId memoId)
        {
            if (userAccountId.StartsWith("M", StringComparison.Ordinal))
            {
                throw new ChallengeValidationErrorMemoAndMuxedAccount("Memo and muxed account (M...) found");
            }

            if (memo != null && memoId.IdValue != (ulong)memo)
            {
                throw new ChallengeValidationErrorInvalidMemoValue("invalid memo value");
            }
        }
        else if (transaction.Memo is not MemoNone)
        {
            throw new ChallengeValidationErrorInvalidMemoType("invalid memo type");
        }
        else if (memo != null)
        {
            throw new ChallengeValidationErrorInvalidMemoValue("missing memo");
        }

        for (var i = 0; i < transaction.Operations.Length; i++)
        {
            var op = transaction.Operations[i];
            
            // Type check instead of discriminant access
            if (op is not ManageDataOperation manageDataOp)
            {
                throw new ChallengeValidationErrorInvalidOperationType($"invalid type of operation {i}");
            }

            // Use SDK property access
            var opSourceAccountId = op.SourceAccount?.AccountId;
            if (opSourceAccountId == null)
            {
                throw new ChallengeValidationErrorInvalidSourceAccount($"invalid source account (is null) in operation[{i}]");
            }

            if (i == 0 && opSourceAccountId != userAccountId)
            {
                throw new ChallengeValidationErrorInvalidSourceAccount($"invalid source account in operation[{i}]");
            }

            // Use clean property access
            var dataName = manageDataOp.Name;
            if (i > 0)
            {
                if (dataName == ClientDomainDataKey)
                {
                    if (opSourceAccountId != clientDomainAccountId)
                    {
                        throw new ChallengeValidationErrorInvalidSourceAccount($"invalid source account in operation[{i}]");
                    }
                }
                else if (opSourceAccountId != _serverSigningKey)
                {
                    throw new ChallengeValidationErrorInvalidSourceAccount($"invalid source account in operation[{i}]");
                }
            }

            if (i == 0 && dataName != _serverHomeDomain + AuthSuffix)
            {
                throw new ChallengeValidationErrorInvalidHomeDomain($"invalid home domain in operation {i}");
            }

            // Use clean property access
            var dataValue = manageDataOp.Value;
            
            // Validate nonce value length for first operation (must be 64 bytes per SEP-0010)
            if (i == 0)
            {
                if (dataValue == null || dataValue.Length != 64)
                {
                    throw new ChallengeValidationErrorInvalidNonceValue(
                        $"invalid nonce value in operation {i}. Expected: 64 bytes, Actual: {dataValue?.Length ?? 0} bytes");
                }
            }
            if (i > 0 && dataName == WebAuthDataKey)
            {
                var uri = new Uri(_authEndpoint);
                var webAuthDomainBytes = Encoding.UTF8.GetBytes(uri.Host);
                if (dataValue == null || !dataValue.SequenceEqual(webAuthDomainBytes))
                {
                    var actualValue = dataValue != null ? Encoding.UTF8.GetString(dataValue) : null;
                    throw new ChallengeValidationErrorInvalidWebAuthDomain(
                        $"invalid web auth domain in operation {i}. Expected: {uri.Host} Actual: {actualValue}");
                }
            }
        }

        // Check time bounds using SDK property
        if (transaction.TimeBounds != null)
        {
            var grace = timeBoundsGracePeriod ?? _gracePeriod;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var minTime = transaction.TimeBounds.MinTime - grace;
            var maxTime = transaction.TimeBounds.MaxTime + grace;

            if (currentTime < minTime || currentTime > maxTime)
            {
                throw new ChallengeValidationErrorInvalidTimeBounds("Invalid transaction, invalid time bounds");
            }
        }

        // Validate server signature - need to access envelope for signatures
        var bytes = Convert.FromBase64String(challengeTransaction);
        var envelopeXdr = TransactionEnvelope.Decode(new XdrDataInputStream(bytes));
        var signatures = envelopeXdr.V1!.Signatures.ToArray();
        
        if (signatures.Length != 1)
        {
            throw new ChallengeValidationErrorInvalidSignature(
                "Invalid transaction envelope, invalid number of signatures");
        }

        var firstSignature = signatures[0];
        var serverKeyPair = KeyPair.FromAccountId(_serverSigningKey);
        var transactionHash = transaction.Hash(_network);
        var valid = serverKeyPair.Verify(transactionHash, firstSignature.Signature.InnerValue);

        if (!valid)
        {
            throw new ChallengeValidationErrorInvalidSignature(
                "Invalid transaction envelope, invalid signature");
        }
    }

    /// <summary>
    ///     Signs a challenge transaction with the provided keypairs.
    /// </summary>
    /// <param name="challengeTransaction">Base64-encoded XDR transaction envelope</param>
    /// <param name="signers">List of keypairs to sign the transaction with</param>
    /// <returns>Base64-encoded XDR transaction envelope with additional signatures</returns>
    public string SignTransaction(string challengeTransaction, ICollection<KeyPair> signers)
    {
        ArgumentException.ThrowIfNullOrEmpty(challengeTransaction);
        ArgumentNullException.ThrowIfNull(signers);

        // Convert to SDK Transaction for easy access
        var transaction = Transactions.Transaction.FromEnvelopeXdr(challengeTransaction);
        var txHash = transaction.Hash(_network);

        // Add signatures using SDK Transaction
        foreach (var signer in signers)
        {
            transaction.Sign(signer, _network);
        }

        // Convert back to XDR envelope for return
        var envelopeXdr = transaction.ToEnvelopeXdr();
        var outputStream = new XdrDataOutputStream();
        TransactionEnvelope.Encode(outputStream, envelopeXdr);
        return Convert.ToBase64String(outputStream.ToArray());
    }

    /// <summary>
    ///     Submits a signed challenge transaction to obtain a JWT token.
    /// </summary>
    /// <param name="base64EnvelopeXdr">The signed challenge transaction as base64-encoded XDR</param>
    /// <returns>The JWT authentication token</returns>
    public async Task<string> SendSignedChallengeAsync(string base64EnvelopeXdr)
    {
        ArgumentException.ThrowIfNullOrEmpty(base64EnvelopeXdr);

        var serverUri = new Uri(_authEndpoint);

        var requestBody = JsonSerializer.Serialize(new { transaction = base64EnvelopeXdr }, JsonOptions.DefaultOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, serverUri)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        if (_httpRequestHeaders != null)
        {
            foreach (var header in _httpRequestHeaders)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip Content-Type as we set it explicitly
                }

                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            switch ((int)response.StatusCode)
            {
                case 200:
                case 400:
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var submitResponse = JsonSerializer.Deserialize<SubmitChallengeResponse>(content, JsonOptions.DefaultOptions);

                    if (submitResponse == null)
                    {
                        throw new SubmitChallengeUnknownResponseException((int)response.StatusCode, content);
                    }

                    if (submitResponse.Error != null)
                    {
                        throw new SubmitChallengeErrorResponseException(submitResponse.Error);
                    }

                    if (submitResponse.Token == null)
                    {
                        throw new SubmitChallengeUnknownResponseException((int)response.StatusCode, content);
                    }

                    return submitResponse.Token;

                case 504:
                    throw new SubmitChallengeTimeoutResponseException();

                default:
                    var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new SubmitChallengeUnknownResponseException((int)response.StatusCode, errorBody);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new SubmitChallengeUnknownResponseException(0, ex.Message);
        }
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

