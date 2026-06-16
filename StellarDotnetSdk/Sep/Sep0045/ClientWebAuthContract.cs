using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0010.Exceptions;
using StellarDotnetSdk.Sep.Sep0045.Exceptions;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
// MissingClientDomainException and NoClientDomainSigningKeyFoundException exist in BOTH
// Sep0010.Exceptions and Sep0045.Exceptions (deliberate per-SEP copies). Since this file imports
// Sep0010.Exceptions (for NoWebAuthServerSigningKeyFoundException), these aliases pin the unqualified
// names to the Sep0045 versions. Keep them — without the aliases the names are ambiguous (CS0104).
using MissingClientDomainException = StellarDotnetSdk.Sep.Sep0045.Exceptions.MissingClientDomainException;
using NoClientDomainSigningKeyFoundException =
    StellarDotnetSdk.Sep.Sep0045.Exceptions.NoClientDomainSigningKeyFoundException;

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
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string>? _httpRequestHeaders;
    private readonly Network _network;
    private readonly bool _ownsHttpClient;
    private readonly string _serverHomeDomain;
    private readonly string _serverSigningKey;
    private readonly uint _signatureExpirationLedgers;
    private readonly string _sorobanRpcUrl;
    private readonly string _webAuthContractId;
    private readonly string _webAuthDomain;

    /// <summary>Creates a new instance with explicit configuration.</summary>
    /// <param name="webAuthDomain">
    ///     The <c>web_auth_domain</c> the server binds challenges to. Servers configure this independently
    ///     of the endpoint URL (the reference server uses the home domain; anchor-platform uses a dedicated
    ///     config value), so it is taken as configuration here rather than derived from
    ///     <paramref name="authEndpoint" />. Defaults to <paramref name="serverHomeDomain" /> when null.
    /// </param>
    /// <param name="httpClient">
    ///     Optional HTTP client to reuse. Credentials placed in this client's
    ///     <see cref="System.Net.Http.HttpClient.DefaultRequestHeaders" /> are sent to <em>every</em> host
    ///     it contacts — including a client domain's stellar.toml (a different origin) during client-domain
    ///     verification. To send credentials only to the auth server, use
    ///     <paramref name="httpRequestHeaders" /> instead; the SDK does not forward those across origins.
    /// </param>
    /// <param name="httpRequestHeaders">
    ///     Optional headers sent with requests to the auth server. The SDK does NOT forward these to the
    ///     client domain's stellar.toml fetch, so they are the safe place for auth-server credentials.
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

        if (!Uri.TryCreate(authEndpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException("authEndpoint must be an absolute URI.", nameof(authEndpoint));
        }
        if (!endpointUri.IsLoopback && endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException(
                "authEndpoint must use https (the SEP-45 challenge and JWT are exchanged over it); " +
                "plain http is allowed only for loopback addresses used in local development.",
                nameof(authEndpoint));
        }

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

    /// <summary>Disposes the internal HttpClient if it was created by this instance.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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
        {
            throw new NoWebAuthContractEndpointFoundException(domain);
        }

        if (string.IsNullOrEmpty(toml.GeneralInformation.WebAuthContractId))
        {
            throw new NoWebAuthContractIdFoundException(domain);
        }

        if (string.IsNullOrEmpty(toml.GeneralInformation.SigningKey))
        {
            throw new NoWebAuthServerSigningKeyFoundException(domain);
        }

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

    /// <summary>Disposes this instance and optionally the internally-owned HttpClient.</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>Fetches the current ledger sequence from the configured Soroban RPC URL.</summary>
    /// <remarks>
    ///     Overridable so unit tests can stub the latest-ledger fetch without a live RPC node.
    /// </remarks>
    internal virtual async Task<uint> GetLatestLedgerSequenceAsync()
    {
        using var rpc = new StellarRpcServer(_sorobanRpcUrl, _httpClient);
        var latest = await rpc.GetLatestLedger().ConfigureAwait(false);
        if (latest.Sequence < 0)
        {
            // Sequence is a signed int on the wire; a negative value is a malformed RPC response that
            // would wrap to a huge uint (and overflow the expiration-ledger addition below), so fail fast.
            throw new InvalidOperationException(
                $"Soroban RPC returned a negative ledger sequence ({latest.Sequence}).");
        }
        return (uint)latest.Sequence;
    }

    /// <summary>
    ///     Fetches a SEP-45 challenge from the configured server for a contract account.
    /// </summary>
    /// <param name="clientAccountId">The contract account being authenticated (C... address).</param>
    /// <param name="homeDomain">Optional home domain if the auth server serves multiple domains.</param>
    /// <param name="clientDomain">Optional client domain for client-domain verification.</param>
    /// <returns>The parsed <see cref="ChallengeForContractsResponse" /> from the server.</returns>
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
        if (homeDomain != null)
        {
            q.Add($"home_domain={Uri.EscapeDataString(homeDomain)}");
        }
        if (clientDomain != null)
        {
            q.Add($"client_domain={Uri.EscapeDataString(clientDomain)}");
        }
        uri.Query = string.Join("&", q);

        using var req = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
        if (_httpRequestHeaders != null)
        {
            foreach (var h in _httpRequestHeaders)
            {
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
            {
                throw new ChallengeForContractsRequestErrorException((int)resp.StatusCode, body);
            }

            ChallengeForContractsResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<ChallengeForContractsResponse>(
                    body, JsonOptions.DefaultOptions);
            }
            catch (JsonException ex)
            {
                throw new ChallengeForContractsRequestErrorException(
                    (int)resp.StatusCode, "Response body was not valid JSON: " + ex.Message, ex);
            }

            if (parsed == null || string.IsNullOrEmpty(parsed.AuthorizationEntries))
            {
                throw new MissingAuthorizationEntriesInChallengeResponseException();
            }

            return parsed;
        }
    }

    /// <summary>
    ///     Validates a SEP-45 challenge: decodes the XDR, enforces structural + map-key rules
    ///     via <see cref="Sep45Challenge.ReadChallenge" />, confirms the client account matches,
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
        {
            throw new InvalidClientAccountException(
                $"Client account mismatch. Expected {clientAccountId}, got {parsed.ClientAccountId}.");
        }

        if (clientDomainAccountId != null)
        {
            if (parsed.ClientDomainAccountId != clientDomainAccountId)
            {
                throw new InvalidClientDomainException(
                    $"Client domain account mismatch. Expected {clientDomainAccountId}, got {parsed.ClientDomainAccountId ?? "<none>"}.");
            }
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
        {
            throw new InvalidServerSignatureException("No entry found for server account.");
        }

        Sep45Challenge.VerifyServerSignature(serverEntry, _serverSigningKey, _network);
    }

    /// <summary>
    ///     Signs SEP-45 authorization entries: the client (contract) entry with every supplied signer,
    ///     and optionally the client-domain entry with a local keypair or a remote signing delegate.
    /// </summary>
    /// <param name="authorizationEntriesXdr">Base64 XDR of the challenge's authorization entries.</param>
    /// <param name="clientAccountId">
    ///     The client account (C... contract address) being authenticated. The entry whose credentials
    ///     address equals this value is signed with every keypair in <paramref name="signers" />.
    /// </param>
    /// <param name="signers">
    ///     Keypairs that authorize the client (contract) entry; a contract may require several. May be
    ///     empty for a contract whose <c>__check_auth</c> requires no signatures (per SEP-45): the client
    ///     entry is then returned with its signature-expiration ledger stamped but no signatures appended.
    ///     An empty collection is intentional and is deliberately not rejected.
    /// </param>
    /// <param name="clientDomainAccountKeyPair">
    ///     Optional local client-domain signing keypair. Self-sufficient: it carries its own account id,
    ///     so <paramref name="clientDomainAccountId" /> is not needed when this is supplied.
    /// </param>
    /// <param name="clientDomainSigningDelegate">
    ///     Optional async delegate invoked to sign the client-domain entry when the keypair isn't
    ///     available locally (typically for remote signing). Must be paired with
    ///     <paramref name="clientDomainAccountId" /> (which locates the entry to sign, since it cannot be
    ///     derived from the delegate) — supplying one without the other throws
    ///     <see cref="InvalidArgumentsException" />. Ignored if <paramref name="clientDomainAccountKeyPair" />
    ///     is provided.
    /// </param>
    /// <param name="clientDomainAccountId">
    ///     The client-domain signing account (G... address) used to locate the client-domain entry when
    ///     signing it via <paramref name="clientDomainSigningDelegate" />. Required with — and only
    ///     meaningful alongside — that delegate.
    /// </param>
    /// <returns>Base64 XDR of the re-encoded, signed authorization entries.</returns>
    /// <exception cref="InvalidArgumentsException">
    ///     Thrown if <paramref name="clientDomainSigningDelegate" /> and
    ///     <paramref name="clientDomainAccountId" /> are not supplied together (absent a local keypair), or
    ///     if an authorization entry's address matches neither the server, the client account, nor the
    ///     client-domain account — i.e. no supplied signer can cover it. Failing here surfaces a clear
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

        // The client-domain entry is signed either locally (clientDomainAccountKeyPair, which carries its
        // own account id) or remotely (clientDomainSigningDelegate). Remote signing additionally needs
        // clientDomainAccountId because that is what locates the entry to hand to the delegate — it cannot
        // be derived from the delegate. So, absent a local keypair, the delegate and the account id are a
        // required pair; supplying exactly one would silently leave the client-domain entry unsigned, so
        // reject that mismatch up front rather than emitting a half-signed challenge the server rejects.
        if (clientDomainAccountKeyPair == null &&
            clientDomainSigningDelegate != null != (clientDomainAccountId != null))
        {
            throw new InvalidArgumentsException(
                "clientDomainSigningDelegate and clientDomainAccountId must be supplied together " +
                "(the delegate signs the client-domain entry; the account id locates it); " +
                "provide both, or use clientDomainAccountKeyPair for local signing.");
        }

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
                entry.Credentials.Address.SignatureExpirationLedger = new Uint32(expiration);
                var hash = Sep45Challenge.ComputeAuthorizationHash(entry, _network);
                foreach (var kp in signers)
                {
                    Sep45Challenge.AppendSignature(entry, kp.PublicKey, kp.Sign(hash));
                }
            }
            else if (clientDomainAccountKeyPair != null && entryAddress == clientDomainAccountKeyPair.AccountId)
            {
                entry.Credentials.Address.SignatureExpirationLedger = new Uint32(expiration);
                var hash = Sep45Challenge.ComputeAuthorizationHash(entry, _network);
                Sep45Challenge.AppendSignature(
                    entry, clientDomainAccountKeyPair.PublicKey, clientDomainAccountKeyPair.Sign(hash));
            }
            else if (clientDomainSigningDelegate != null && clientDomainAccountId != null &&
                     entryAddress == clientDomainAccountId)
            {
                entry.Credentials.Address.SignatureExpirationLedger = new Uint32(expiration);
                // The delegate's return is non-nullable by contract (see ClientDomainEntrySigningDelegate),
                // so we trust that annotation rather than re-guarding its result here.
                entries[i] = await clientDomainSigningDelegate(entry).ConfigureAwait(false);
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
            var json = JsonSerializer.Serialize(
                new { authorization_entries = signedAuthorizationEntriesXdr }, JsonOptions.DefaultOptions);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        if (_httpRequestHeaders != null)
        {
            foreach (var h in _httpRequestHeaders)
            {
                if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
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
                        parsed = JsonSerializer.Deserialize<SubmitChallengeForContractsResponse>(
                            body, JsonOptions.DefaultOptions);
                    }
                    catch (JsonException ex)
                    {
                        throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body, ex);
                    }

                    if (parsed == null)
                    {
                        throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body);
                    }
                    if (!string.IsNullOrEmpty(parsed.Error))
                    {
                        throw new SubmitSignedChallengeForContractsErrorResponseException(parsed.Error!);
                    }
                    if (string.IsNullOrEmpty(parsed.Token))
                    {
                        throw new SubmitSignedChallengeForContractsUnknownResponseException(status, body);
                    }
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
    /// <param name="signers">
    ///     Keypairs that authorize the client (contract) entry. May be empty for a contract whose
    ///     <c>__check_auth</c> requires no signatures (per SEP-45).
    /// </param>
    /// <param name="homeDomain">Optional home domain override (for auth servers serving multiple home domains).</param>
    /// <param name="clientDomain">Optional client domain.</param>
    /// <param name="clientDomainAccountKeyPair">Optional local client-domain signing keypair.</param>
    /// <param name="clientDomainSigningDelegate">
    ///     Optional remote client-domain signing delegate. Requires <paramref name="clientDomain" /> to
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

        var resolvedClientDomainAccountId = clientDomainAccountKeyPair?.AccountId;
        if (resolvedClientDomainAccountId == null && clientDomainSigningDelegate != null)
        {
            if (string.IsNullOrEmpty(clientDomain))
            {
                throw new MissingClientDomainException();
            }

            // Do not forward the auth server's request headers (which may carry credentials) to the
            // client domain — it is a different origin.
            var clientToml = await StellarToml
                .FromDomainAsync(clientDomain, null, null, _httpClient)
                .ConfigureAwait(false);
            if (string.IsNullOrEmpty(clientToml.GeneralInformation.SigningKey))
            {
                throw new NoClientDomainSigningKeyFoundException(clientDomain);
            }
            resolvedClientDomainAccountId = clientToml.GeneralInformation.SigningKey;
        }

        var challenge = await GetChallengeAsync(clientAccountId, homeDomain, clientDomain)
            .ConfigureAwait(false);

        // Fail fast with a clear error if the server issued the challenge for a different network. The
        // field is optional (SEP-45: "optional but recommended"), so validate only when present —
        // matching the Flutter peer SDK. A wrong-network challenge would otherwise surface later as an
        // opaque server-signature verification failure.
        if (!string.IsNullOrEmpty(challenge.NetworkPassphrase) &&
            challenge.NetworkPassphrase != _network.NetworkPassphrase)
        {
            throw new InvalidNetworkPassphraseException(
                $"Challenge network passphrase mismatch. Expected '{_network.NetworkPassphrase}', " +
                $"got '{challenge.NetworkPassphrase}'.");
        }

        var xdr = challenge.AuthorizationEntries!;

        ValidateChallenge(xdr, clientAccountId, resolvedClientDomainAccountId, homeDomain);

        var signed = await SignAuthorizationEntriesAsync(
            xdr, clientAccountId, signers, clientDomainAccountKeyPair,
            clientDomainSigningDelegate, resolvedClientDomainAccountId).ConfigureAwait(false);

        return await SendSignedChallengeAsync(signed).ConfigureAwait(false);
    }
}