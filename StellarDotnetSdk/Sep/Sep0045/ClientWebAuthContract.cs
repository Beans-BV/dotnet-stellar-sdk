using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0045.Exceptions;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>
///     Client-side implementation of SEP-45 (Web Authentication for Contract Accounts).
///     Orchestrates challenge fetching, validation, signing, and submission.
/// </summary>
public class ClientWebAuthContract : IDisposable
{
    /// <summary>Default number of ledgers added to the latest ledger to compute signature expiration.</summary>
    public const uint DefaultSignatureExpirationLedgers = 10;

    private readonly string _authEndpoint;
    private readonly string _webAuthContractId;
    private readonly Network _network;
    private readonly string _serverSigningKey;
    private readonly string _serverHomeDomain;
    private readonly string _webAuthDomain;
    private readonly string _sorobanRpcUrl;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly uint _signatureExpirationLedgers;
    private readonly bool _ownsHttpClient;

    /// <summary>Creates a new instance with explicit configuration.</summary>
    /// <param name="webAuthDomain">
    ///     The <c>web_auth_domain</c> the server binds challenges to. Servers configure this independently
    ///     of the endpoint URL (the reference server uses the home domain; anchor-platform uses a dedicated
    ///     config value), so it is taken as configuration here rather than derived from
    ///     <paramref name="authEndpoint"/>. Defaults to <paramref name="serverHomeDomain"/> when null.
    /// </param>
    public ClientWebAuthContract(
        string authEndpoint,
        string webAuthContractId,
        Network network,
        string serverSigningKey,
        string serverHomeDomain,
        string sorobanRpcUrl,
        string? webAuthDomain = null,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        uint? signatureExpirationLedgers = null,
        HttpResilienceOptions? resilienceOptions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(authEndpoint);
        ArgumentException.ThrowIfNullOrEmpty(webAuthContractId);
        ArgumentNullException.ThrowIfNull(network);
        ArgumentException.ThrowIfNullOrEmpty(serverSigningKey);
        ArgumentException.ThrowIfNullOrEmpty(serverHomeDomain);
        ArgumentException.ThrowIfNullOrEmpty(sorobanRpcUrl);

        _authEndpoint = authEndpoint;
        _webAuthContractId = webAuthContractId;
        _network = network;
        _serverSigningKey = serverSigningKey;
        _serverHomeDomain = serverHomeDomain;
        _webAuthDomain = string.IsNullOrEmpty(webAuthDomain) ? serverHomeDomain : webAuthDomain;
        _sorobanRpcUrl = sorobanRpcUrl;
        _httpRequestHeaders = httpRequestHeaders;
        _signatureExpirationLedgers = signatureExpirationLedgers ?? DefaultSignatureExpirationLedgers;

        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions);
            _ownsHttpClient = true;
        }
    }

    /// <summary>Discovers configuration from the domain's stellar.toml and constructs an instance.</summary>
    public static async Task<ClientWebAuthContract> FromDomainAsync(
        string domain,
        Network network,
        string sorobanRpcUrl,
        HttpResilienceOptions? resilienceOptions = null,
        string? bearerToken = null,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        string? webAuthDomain = null)
    {
        var toml = await StellarToml
            .FromDomainAsync(domain, resilienceOptions, bearerToken, httpClient, httpRequestHeaders)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(toml.GeneralInformation.WebAuthForContractsEndpoint))
            throw new NoWebAuthContractEndpointFoundException(domain);

        if (string.IsNullOrEmpty(toml.GeneralInformation.WebAuthContractId))
            throw new NoWebAuthContractIdFoundException(domain);

        if (string.IsNullOrEmpty(toml.GeneralInformation.SigningKey))
            throw new Sep0010.Exceptions.NoWebAuthServerSigningKeyFoundException(domain);

        return new ClientWebAuthContract(
            toml.GeneralInformation.WebAuthForContractsEndpoint!,
            toml.GeneralInformation.WebAuthContractId!,
            network,
            toml.GeneralInformation.SigningKey!,
            domain,
            sorobanRpcUrl,
            webAuthDomain,
            httpClient,
            httpRequestHeaders,
            resilienceOptions: resilienceOptions);
    }

    /// <summary>Disposes the internal HttpClient if it was created by this instance.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes this instance and optionally the internally-owned HttpClient.</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _ownsHttpClient)
            _httpClient.Dispose();
    }

    /// <summary>Fetches the current ledger sequence from the configured Soroban RPC URL.</summary>
    /// <remarks>
    ///     Overridable so unit tests can stub the latest-ledger fetch without a live RPC node.
    /// </remarks>
    internal virtual async Task<uint> GetLatestLedgerSequenceAsync()
    {
        using var rpc = new SorobanServer(_sorobanRpcUrl, _httpClient);
        var latest = await rpc.GetLatestLedger().ConfigureAwait(false);
        return (uint)latest.Sequence;
    }

    /// <summary>
    ///     Fetches a SEP-45 challenge from the configured server for a contract account.
    /// </summary>
    /// <param name="clientAccountId">The contract account being authenticated (C... address).</param>
    /// <param name="homeDomain">Optional home domain if the auth server serves multiple domains.</param>
    /// <param name="clientDomain">Optional client domain for client-domain verification.</param>
    /// <returns>The parsed <see cref="ChallengeForContractsResponse"/> from the server.</returns>
    /// <exception cref="ChallengeForContractsRequestErrorException">
    ///     Thrown if the server returns a non-2xx status, or the body is malformed/unparseable.
    /// </exception>
    /// <exception cref="MissingAuthorizationEntriesInChallengeResponseException">
    ///     Thrown when the server returned a valid JSON body but without an <c>authorization_entries</c> field.
    /// </exception>
    public async Task<ChallengeForContractsResponse> GetChallengeAsync(
        string clientAccountId,
        string? homeDomain = null,
        string? clientDomain = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientAccountId);

        var uri = new UriBuilder(_authEndpoint);
        var q = new List<string> { $"account={Uri.EscapeDataString(clientAccountId)}" };
        if (homeDomain != null) q.Add($"home_domain={Uri.EscapeDataString(homeDomain)}");
        if (clientDomain != null) q.Add($"client_domain={Uri.EscapeDataString(clientDomain)}");
        uri.Query = string.Join("&", q);

        using var req = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
        if (_httpRequestHeaders != null)
        {
            foreach (var h in _httpRequestHeaders)
                req.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        HttpResponseMessage resp;
        try
        {
            resp = await _httpClient.SendAsync(req).ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            throw new ChallengeForContractsRequestErrorException(
                0, $"Request to '{_authEndpoint}' timed out before a response was received.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ChallengeForContractsRequestErrorException(
                0, $"Failed to reach '{_authEndpoint}': {ex.Message}", ex);
        }

        using (resp)
        {
            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw new ChallengeForContractsRequestErrorException((int)resp.StatusCode, body);

            ChallengeForContractsResponse? parsed;
            try
            {
                parsed = System.Text.Json.JsonSerializer.Deserialize<ChallengeForContractsResponse>(
                    body, StellarDotnetSdk.Converters.JsonOptions.DefaultOptions);
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new ChallengeForContractsRequestErrorException(
                    (int)resp.StatusCode, "Response body was not valid JSON: " + ex.Message, ex);
            }

            if (parsed == null || string.IsNullOrEmpty(parsed.AuthorizationEntries))
                throw new MissingAuthorizationEntriesInChallengeResponseException();

            return parsed;
        }
    }

    /// <summary>
    ///     Validates a SEP-45 challenge: decodes the XDR, enforces structural + map-key rules
    ///     via <see cref="Sep45Challenge.ReadChallenge"/>, confirms the client account matches,
    ///     enforces the client-domain invariant, and verifies the server's Ed25519 signature.
    /// </summary>
    /// <param name="authorizationEntriesXdr">Base64-encoded XDR of the challenge's authorization entries.</param>
    /// <param name="clientAccountId">The expected client contract account ID (C... address).</param>
    /// <param name="clientDomainAccountId">
    ///     Optional expected client-domain signing account. If null, the challenge must not contain
    ///     client-domain keys.
    /// </param>
    /// <param name="homeDomain">
    ///     Optional home domain the challenge was requested for. Defaults to the configured server home
    ///     domain; supply this when the auth server serves multiple home domains so the challenge's
    ///     <c>home_domain</c> is validated against the one actually requested.
    /// </param>
    public void ValidateChallenge(
        string authorizationEntriesXdr,
        string clientAccountId,
        string? clientDomainAccountId = null,
        string? homeDomain = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(authorizationEntriesXdr);
        ArgumentException.ThrowIfNullOrEmpty(clientAccountId);

        var parsed = Sep45Challenge.ReadChallenge(
            authorizationEntriesXdr,
            _serverSigningKey,
            _webAuthContractId,
            new[] { string.IsNullOrEmpty(homeDomain) ? _serverHomeDomain : homeDomain },
            _webAuthDomain);

        if (parsed.ClientAccountId != clientAccountId)
            throw new InvalidClientAccountException(
                $"Client account mismatch. Expected {clientAccountId}, got {parsed.ClientAccountId}.");

        if (clientDomainAccountId != null)
        {
            if (parsed.ClientDomainAccountId != clientDomainAccountId)
                throw new InvalidClientDomainException(
                    $"Client domain account mismatch. Expected {clientDomainAccountId}, got {parsed.ClientDomainAccountId ?? "<none>"}.");
        }
        else if (parsed.ClientDomainAccountId != null)
        {
            throw new InvalidClientDomainException(
                "Challenge contains client_domain but no clientDomainAccountId was supplied.");
        }

        // Locate server entry by credentials address.
        SorobanAuthorizationEntry? serverEntry = null;
        foreach (var e in parsed.Entries)
        {
            if (CredentialsAddressToStrKey(e) == _serverSigningKey)
            {
                serverEntry = e;
                break;
            }
        }
        if (serverEntry == null)
            throw new InvalidServerSignatureException("No entry found for server account.");

        Sep45Challenge.VerifyServerSignature(serverEntry, _serverSigningKey, _network);
    }

    /// <summary>
    ///     Signs SEP-45 authorization entries: the client (contract) entry with every supplied signer,
    ///     and optionally the client-domain entry with a local keypair or a remote signing delegate.
    /// </summary>
    /// <param name="authorizationEntriesXdr">Base64 XDR of the challenge's authorization entries.</param>
    /// <param name="clientAccountId">
    ///     The client account (C... contract address) being authenticated. The entry whose credentials
    ///     address equals this value is signed with every keypair in <paramref name="signers"/>.
    /// </param>
    /// <param name="signers">Keypairs that authorize the client entry (a contract may require several).</param>
    /// <param name="clientDomainAccountKeyPair">Optional local client-domain signing keypair.</param>
    /// <param name="clientDomainSigningDelegate">
    ///     Optional async delegate invoked to sign the client-domain entry when the keypair isn't
    ///     available locally (typically for remote signing). Requires <paramref name="clientDomainAccountId"/>
    ///     so the correct entry can be located. Ignored if <paramref name="clientDomainAccountKeyPair"/> is provided.
    /// </param>
    /// <param name="clientDomainAccountId">
    ///     The client-domain signing account (G... address) used to locate the client-domain entry when
    ///     signing it via <paramref name="clientDomainSigningDelegate"/>.
    /// </param>
    /// <returns>Base64 XDR of the re-encoded, signed authorization entries.</returns>
    /// <exception cref="InvalidArgumentsException">
    ///     Thrown if an authorization entry's address matches neither the server, the client account, nor
    ///     the client-domain account — i.e. no supplied signer can cover it. Failing here surfaces a clear
    ///     local error instead of letting the server reject the half-signed challenge with an opaque message.
    /// </exception>
    public async Task<string> SignAuthorizationEntriesAsync(
        string authorizationEntriesXdr,
        string clientAccountId,
        ICollection<KeyPair> signers,
        KeyPair? clientDomainAccountKeyPair = null,
        ClientDomainEntrySigningDelegate? clientDomainSigningDelegate = null,
        string? clientDomainAccountId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(authorizationEntriesXdr);
        ArgumentException.ThrowIfNullOrEmpty(clientAccountId);
        ArgumentNullException.ThrowIfNull(signers);

        var entries = Sep45Challenge.DecodeAuthorizationEntries(authorizationEntriesXdr);
        var latest = await GetLatestLedgerSequenceAsync().ConfigureAwait(false);
        var expiration = latest + _signatureExpirationLedgers;

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.Credentials.Discriminant.InnerValue !=
                SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS)
            {
                continue;
            }

            var entryAddress = CredentialsAddressToStrKey(entry);

            if (entryAddress == _serverSigningKey)
            {
                continue; // server entry is already signed by the server — do not touch
            }

            if (entryAddress == clientAccountId)
            {
                // Client (contract) entry: authorize with every supplied signer. The signature
                // expiration must be stamped before hashing because it is part of the signed preimage.
                entry.Credentials.Address.SignatureExpirationLedger = new Xdr.Uint32(expiration);
                var hash = Sep45Challenge.ComputeAuthorizationHash(entry, _network);
                foreach (var kp in signers)
                    Sep45Challenge.AppendSignature(entry, kp.PublicKey, kp.Sign(hash));
            }
            else if (clientDomainAccountKeyPair != null && entryAddress == clientDomainAccountKeyPair.AccountId)
            {
                entry.Credentials.Address.SignatureExpirationLedger = new Xdr.Uint32(expiration);
                var hash = Sep45Challenge.ComputeAuthorizationHash(entry, _network);
                Sep45Challenge.AppendSignature(
                    entry, clientDomainAccountKeyPair.PublicKey, clientDomainAccountKeyPair.Sign(hash));
            }
            else if (clientDomainSigningDelegate != null && clientDomainAccountId != null &&
                     entryAddress == clientDomainAccountId)
            {
                entry.Credentials.Address.SignatureExpirationLedger = new Xdr.Uint32(expiration);
                var signedEntry = await clientDomainSigningDelegate(entry).ConfigureAwait(false);
                entries[i] = signedEntry
                    ?? throw new InvalidArgumentsException("clientDomainSigningDelegate returned null.");
            }
            else
            {
                throw new InvalidArgumentsException(
                    $"No signer available for authorization entry with address '{entryAddress}'. " +
                    $"Expected the client account '{clientAccountId}'" +
                    (clientDomainAccountKeyPair != null || clientDomainAccountId != null
                        ? $" or the client-domain account '{clientDomainAccountKeyPair?.AccountId ?? clientDomainAccountId}'."
                        : "."));
            }
        }

        return Sep45Challenge.EncodeAuthorizationEntries(entries);
    }

    /// <summary>
    ///     POSTs the signed authorization entries XDR to the SEP-45 server and returns the JWT.
    /// </summary>
    /// <param name="signedAuthorizationEntriesXdr">Base64 XDR of fully-signed authorization entries.</param>
    /// <param name="useFormUrlEncoded">
    ///     If true (default), posts as <c>application/x-www-form-urlencoded</c> with key
    ///     <c>authorization_entries</c>. If false, posts JSON <c>{"authorization_entries":"..."}</c>.
    /// </param>
    /// <returns>The JWT token on success.</returns>
    public async Task<string> SendSignedChallengeAsync(
        string signedAuthorizationEntriesXdr, bool useFormUrlEncoded = true)
    {
        ArgumentException.ThrowIfNullOrEmpty(signedAuthorizationEntriesXdr);

        using var req = new HttpRequestMessage(HttpMethod.Post, new Uri(_authEndpoint));
        if (useFormUrlEncoded)
        {
            req.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("authorization_entries", signedAuthorizationEntriesXdr),
            });
        }
        else
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                new { authorization_entries = signedAuthorizationEntriesXdr });
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        if (_httpRequestHeaders != null)
        {
            foreach (var h in _httpRequestHeaders)
            {
                if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;
                req.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
        }

        HttpResponseMessage resp;
        try
        {
            resp = await _httpClient.SendAsync(req).ConfigureAwait(false);
        }
        catch (TaskCanceledException ex)
        {
            throw new SubmitSignedChallengeForContractsTimeoutResponseException(ex);
        }
        catch (HttpRequestException ex)
        {
            throw new SubmitSignedChallengeForContractsUnknownResponseException(
                0, $"Failed to reach '{_authEndpoint}': {ex.Message}", ex);
        }

        using (resp)
        {
            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var status = (int)resp.StatusCode;
            switch (status)
            {
                case 200:
                case 400:
                    SubmitChallengeForContractsResponse? parsed;
                    try
                    {
                        parsed = System.Text.Json.JsonSerializer.Deserialize<SubmitChallengeForContractsResponse>(
                            body, StellarDotnetSdk.Converters.JsonOptions.DefaultOptions);
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body, ex);
                    }

                    if (parsed == null)
                        throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body);
                    if (!string.IsNullOrEmpty(parsed.Error))
                        throw new SubmitSignedChallengeForContractsErrorResponseException(parsed.Error!);
                    if (string.IsNullOrEmpty(parsed.Token))
                        throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body);
                    return parsed.Token!;

                case 504:
                    throw new SubmitSignedChallengeForContractsTimeoutResponseException();

                default:
                    throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body);
            }
        }
    }

    private static string CredentialsAddressToStrKey(SorobanAuthorizationEntry entry)
    {
        var soroban = ScAddress.FromXdr(entry.Credentials.Address.Address);
        return soroban switch
        {
            ScAccountId acc => acc.InnerValue,
            ScContractId con => con.InnerValue,
            _ => throw new InvalidSep45ChallengeException(
                $"Unsupported credentials address type '{soroban.GetType().Name}'; " +
                "only account (G...) and contract (C...) addresses are valid in SEP-45 challenges."),
        };
    }

    /// <summary>
    ///     End-to-end SEP-45 flow: fetch challenge, validate, sign, submit, return JWT.
    /// </summary>
    /// <param name="clientAccountId">Contract account being authenticated (C... address).</param>
    /// <param name="signers">Keypairs whose account IDs cover the client entries in the challenge.</param>
    /// <param name="homeDomain">Optional home domain override (for auth servers serving multiple home domains).</param>
    /// <param name="clientDomain">Optional client domain.</param>
    /// <param name="clientDomainAccountKeyPair">Optional local client-domain signing keypair.</param>
    /// <param name="clientDomainSigningDelegate">
    ///     Optional remote client-domain signing delegate. Requires <paramref name="clientDomain"/> to
    ///     be supplied so the SDK can resolve the client's signing public key from its stellar.toml.
    /// </param>
    /// <returns>The JWT session token issued by the server.</returns>
    public async Task<string> JwtTokenAsync(
        string clientAccountId,
        ICollection<KeyPair> signers,
        string? homeDomain = null,
        string? clientDomain = null,
        KeyPair? clientDomainAccountKeyPair = null,
        ClientDomainEntrySigningDelegate? clientDomainSigningDelegate = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientAccountId);
        ArgumentNullException.ThrowIfNull(signers);

        string? resolvedClientDomainAccountId = clientDomainAccountKeyPair?.AccountId;
        if (resolvedClientDomainAccountId == null && clientDomainSigningDelegate != null)
        {
            if (string.IsNullOrEmpty(clientDomain))
                throw new MissingClientDomainException();

            // Do not forward the auth server's request headers (which may carry credentials) to the
            // client domain — it is a different origin.
            var clientToml = await StellarToml
                .FromDomainAsync(clientDomain, null, null, _httpClient)
                .ConfigureAwait(false);
            if (string.IsNullOrEmpty(clientToml.GeneralInformation.SigningKey))
                throw new NoClientDomainSigningKeyFoundException(clientDomain);
            resolvedClientDomainAccountId = clientToml.GeneralInformation.SigningKey;
        }

        var challenge = await GetChallengeAsync(clientAccountId, homeDomain, clientDomain)
            .ConfigureAwait(false);
        var xdr = challenge.AuthorizationEntries!;

        ValidateChallenge(xdr, clientAccountId, resolvedClientDomainAccountId, homeDomain);

        var signed = await SignAuthorizationEntriesAsync(
            xdr, clientAccountId, signers, clientDomainAccountKeyPair,
            clientDomainSigningDelegate, resolvedClientDomainAccountId).ConfigureAwait(false);

        return await SendSignedChallengeAsync(signed).ConfigureAwait(false);
    }
}
