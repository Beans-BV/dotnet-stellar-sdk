using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Compatibility;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0007.Exceptions;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>
///     SEP-7 (URI Scheme to facilitate delegated signing): builds, parses, validates, signs and verifies
///     <c>web+stellar:</c> request URIs, and hands the signed transaction of a <c>tx</c> request on to its
///     callback or to Horizon.
/// </summary>
/// <remarks>
///     <para>
///         The static members work offline. The instance members fetch the origin domain's stellar.toml or POST to
///         a callback with the <see cref="HttpClient" /> given to the constructor; submitting to Horizon goes
///         through the <see cref="Server" /> passed in, with that server's own client.
///     </para>
///     <para>
///         Request signatures cover the URI string exactly as sent (minus the <c>signature</c> parameter), so
///         verification works on the received string and never re-encodes it.
///     </para>
/// </remarks>
public class UriScheme : IDisposable
{
    /// <summary>The scheme every SEP-7 URI starts with.</summary>
    public const string SchemePrefix = "web+stellar:";

    /// <summary>The operation name of a request to sign a transaction.</summary>
    public const string OperationTx = "tx";

    /// <summary>The operation name of a request to make a payment.</summary>
    public const string OperationPay = "pay";

    /// <summary>The prefix of the <c>callback</c> parameter; the only callback type SEP-7 defines.</summary>
    public const string CallbackUrlPrefix = "url:";

    /// <summary>The text prepended to the URI when building the request-signature payload.</summary>
    public const string SignaturePayloadPrefix = "stellar.sep.7 - URI Scheme";

    /// <summary>The longest <c>msg</c> SEP-7 allows, in characters before URL-encoding.</summary>
    public const int MaxMessageLength = 300;

    /// <summary>The deepest nesting of <c>chain</c> parameters SEP-7 allows.</summary>
    public const int MaxChainNestingLevels = 7;

    /// <summary>Separates the fields section of a <c>replace</c> value from its hints section.</summary>
    public const char ReplaceHintDelimiter = ';';

    /// <summary>Separates the entries within each section of a <c>replace</c> value.</summary>
    public const char ReplaceListDelimiter = ',';

    /// <summary>Separates a path from its identifier, and an identifier from its hint, in a <c>replace</c> value.</summary>
    public const char ReplaceIdDelimiter = ':';

    /// <summary>
    ///     Upper bound on a stellar.toml or callback response body. This stops a hostile or malfunctioning server
    ///     from exhausting memory by streaming an unbounded body into a string; 512 KiB is generous headroom over
    ///     a real stellar.toml (SEP-1 asks for at most 100 KB).
    /// </summary>
    private const int MaxResponseBodyBytes = 512 * 1024;

    /// <summary>How many redirects of the stellar.toml request are followed.</summary>
    private const int MaxStellarTomlRedirects = 5;

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>Creates a new instance.</summary>
    /// <param name="httpClient">
    ///     <para>
    ///         Optional HTTP client to reuse for stellar.toml fetches and callback POSTs. Credentials placed in its
    ///         <see cref="System.Net.Http.HttpClient.DefaultRequestHeaders" /> are sent to every origin domain and
    ///         callback URL a request names, which are chosen by whoever built the request; do not put secrets there.
    ///     </para>
    ///     <para>
    ///         Give it a handler with automatic redirects turned off (<c>AllowAutoRedirect = false</c>), as the
    ///         client the SDK creates has; a default <see cref="HttpClient" /> and
    ///         <see cref="DefaultStellarSdkHttpClient" /> follow redirects. A client that follows redirects on its
    ///         own turns a callback POST answered with 301/302/303 into a GET, or re-POSTs the signed transaction to
    ///         wherever a 307/308 points, before this class can intervene. <see cref="SubmitToCallbackAsync" />
    ///         detects that afterwards on a best-effort basis and throws, and for the stellar.toml fetch the
    ///         client's own redirect limits apply instead of this class's (5 hops, https only).
    ///     </para>
    ///     <para>
    ///         Its <see cref="System.Net.Http.HttpClient.Timeout" /> bounds each stellar.toml fetch and callback POST
    ///         as a whole, including reading the response body.
    ///     </para>
    /// </param>
    /// <param name="resilienceOptions">
    ///     Optional retry/timeout options for the <see cref="HttpClient" /> the SDK creates. Ignored when
    ///     <paramref name="httpClient" /> is supplied — that client is used as-is. Retries apply to the callback
    ///     POST too: with <c>MaxRetryCount</c> above zero a signed transaction is sent again after a connection
    ///     failure (and after any status listed in <c>RetryHttpStatusCodes</c> whose method gate admits POST), so a
    ///     callback may receive it more than once. Leave retries off here unless every callback you POST to is
    ///     idempotent.
    /// </param>
    public UriScheme(HttpClient? httpClient = null, HttpResilienceOptions? resilienceOptions = null)
    {
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilienceOptions,
                innerHandler: CreateNonRedirectingHandler());
            _ownsHttpClient = true;
        }
    }

    /// <summary>Disposes the internal HttpClient if it was created by this instance.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes this instance and optionally the internally-owned HttpClient.</summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    ///     Builds a <c>web+stellar:tx</c> URI asking a wallet to sign a transaction. Parameters are emitted in the
    ///     order SEP-7 lists them and URL-encoded. Sign the result with <see cref="SignUri" /> when it carries an
    ///     <paramref name="originDomain" />.
    /// </summary>
    /// <param name="transactionEnvelopeXdr">The base64 XDR <c>TransactionEnvelope</c> to sign (<c>xdr</c>).</param>
    /// <param name="replacements">
    ///     Fields of the transaction the wallet should fill in (<c>replace</c>). Identifiers and hints must not
    ///     contain <c>:</c>, <c>,</c> or <c>;</c>; see <see cref="ReplacementsToString" />.
    /// </param>
    /// <param name="callback">
    ///     The URL the wallet should POST the signed transaction to (<c>callback</c>). The <c>url:</c> prefix is
    ///     added when missing. Without a callback the wallet submits the transaction to the network itself. Null
    ///     leaves it out; an empty string is rejected.
    /// </param>
    /// <param name="publicKey">The account id that should sign the transaction (<c>pubkey</c>); null leaves it out, empty is rejected.</param>
    /// <param name="chain">An embedded SEP-7 URI that caused this request (<c>chain</c>); null or empty leaves it out.</param>
    /// <param name="message">A message for the user, at most 300 characters (<c>msg</c>); null or empty leaves it out.</param>
    /// <param name="networkPassphrase">
    ///     The network passphrase, when not the public network (<c>network_passphrase</c>); null for the public
    ///     network. An empty string is rejected rather than read as "public network".
    /// </param>
    /// <param name="originDomain">The fully qualified domain the request comes from (<c>origin_domain</c>); null leaves it out, empty is rejected.</param>
    /// <returns>The request URI.</returns>
    /// <exception cref="ArgumentException">Thrown when a value would make the URI invalid; the message says which.</exception>
    public static string GenerateSignTransactionUri(
        string transactionEnvelopeXdr,
        IEnumerable<Sep7Replacement>? replacements = null,
        string? callback = null,
        string? publicKey = null,
        string? chain = null,
        string? message = null,
        string? networkPassphrase = null,
        string? originDomain = null)
    {
        Throw.IfNullOrEmpty(transactionEnvelopeXdr, nameof(transactionEnvelopeXdr));

        var parameters = new List<KeyValuePair<string, string?>>
        {
            new(Sep7Parameters.Xdr, transactionEnvelopeXdr),
            new(Sep7Parameters.Replace, replacements == null ? null : ReplacementsToString(replacements)),
            new(Sep7Parameters.Callback, NormalizeCallback(callback)),
            new(Sep7Parameters.Pubkey, publicKey),
            new(Sep7Parameters.Chain, chain),
            new(Sep7Parameters.Msg, message),
            new(Sep7Parameters.NetworkPassphrase, networkPassphrase),
            new(Sep7Parameters.OriginDomain, originDomain),
        };
        return BuildUri(OperationTx, parameters);
    }

    /// <summary>
    ///     Builds a <c>web+stellar:tx</c> URI asking a wallet to sign <paramref name="transaction" />, encoded with
    ///     its current signatures, if any (an unsigned transaction is allowed). See the <see cref="string" />
    ///     overload for the other parameters.
    /// </summary>
    /// <param name="transaction">The transaction to sign.</param>
    /// <param name="replacements">Fields of the transaction the wallet should fill in (<c>replace</c>).</param>
    /// <param name="callback">The URL the wallet should POST the signed transaction to (<c>callback</c>); null leaves it out, empty is rejected.</param>
    /// <param name="publicKey">The account id that should sign the transaction (<c>pubkey</c>); null leaves it out, empty is rejected.</param>
    /// <param name="chain">An embedded SEP-7 URI that caused this request (<c>chain</c>); null or empty leaves it out.</param>
    /// <param name="message">A message for the user, at most 300 characters (<c>msg</c>); null or empty leaves it out.</param>
    /// <param name="networkPassphrase">
    ///     The network passphrase, when not the public network (<c>network_passphrase</c>); null for the public
    ///     network. An empty string is rejected rather than read as "public network".
    /// </param>
    /// <param name="originDomain">The fully qualified domain the request comes from (<c>origin_domain</c>); null leaves it out, empty is rejected.</param>
    /// <returns>The request URI.</returns>
    /// <exception cref="ArgumentException">Thrown when a value would make the URI invalid; the message says which.</exception>
    public static string GenerateSignTransactionUri(
        TransactionBase transaction,
        IEnumerable<Sep7Replacement>? replacements = null,
        string? callback = null,
        string? publicKey = null,
        string? chain = null,
        string? message = null,
        string? networkPassphrase = null,
        string? originDomain = null)
    {
        Throw.IfNull(transaction, nameof(transaction));
        // SEP-7 requests usually carry an unsigned transaction, which ToEnvelopeXdrBase64 refuses to encode.
        var envelope = transaction.Signatures.Count > 0
            ? transaction.ToEnvelopeXdrBase64()
            : transaction.ToUnsignedEnvelopeXdrBase64();
        return GenerateSignTransactionUri(envelope, replacements, callback, publicKey, chain, message,
            networkPassphrase, originDomain);
    }

    /// <summary>
    ///     Builds a <c>web+stellar:pay</c> URI asking a wallet to make a payment. Parameters are emitted in the
    ///     order SEP-7 lists them and URL-encoded. Sign the result with <see cref="SignUri" /> when it carries an
    ///     <paramref name="originDomain" />.
    /// </summary>
    /// <param name="destination">
    ///     The account id (G...), muxed account (M...), contract id (C...) or federation address (<c>name*domain</c>)
    ///     to pay.
    /// </param>
    /// <param name="amount">
    ///     The amount, e.g. <c>"120.1234567"</c>; the wallet asks the user when omitted. Null or empty leaves it out.
    /// </param>
    /// <param name="assetCode">
    ///     The asset code; omit together with <paramref name="assetIssuer" /> for XLM. Null or empty leaves it out.
    /// </param>
    /// <param name="assetIssuer">
    ///     The asset issuer; omit together with <paramref name="assetCode" /> for XLM. Null or empty leaves it out.
    /// </param>
    /// <param name="memo">
    ///     The memo. Encoded as SEP-7 specifies: text as-is, ids in decimal, hash and return-hash memos as base64
    ///     of their 32 bytes, with the matching <c>memo_type</c>. A <see cref="MemoNone" /> is omitted; an empty
    ///     text memo cannot be expressed and is rejected.
    /// </param>
    /// <param name="callback">The URL the wallet should POST the signed transaction to; <c>url:</c> is added when missing. Null leaves it out, empty is rejected.</param>
    /// <param name="message">A message for the user, at most 300 characters (<c>msg</c>); null or empty leaves it out.</param>
    /// <param name="networkPassphrase">
    ///     The network passphrase, when not the public network (<c>network_passphrase</c>); null for the public
    ///     network. An empty string is rejected rather than read as "public network".
    /// </param>
    /// <param name="originDomain">The fully qualified domain the request comes from (<c>origin_domain</c>); null leaves it out, empty is rejected.</param>
    /// <returns>The request URI.</returns>
    /// <exception cref="ArgumentException">Thrown when a value would make the URI invalid; the message says which.</exception>
    public static string GeneratePayOperationUri(
        string destination,
        string? amount = null,
        string? assetCode = null,
        string? assetIssuer = null,
        Memo? memo = null,
        string? callback = null,
        string? message = null,
        string? networkPassphrase = null,
        string? originDomain = null)
    {
        Throw.IfNullOrEmpty(destination, nameof(destination));

        var (memoValue, memoType) = EncodeMemo(memo);
        var parameters = new List<KeyValuePair<string, string?>>
        {
            new(Sep7Parameters.Destination, destination),
            new(Sep7Parameters.Amount, amount),
            new(Sep7Parameters.AssetCode, assetCode),
            new(Sep7Parameters.AssetIssuer, assetIssuer),
            new(Sep7Parameters.Memo, memoValue),
            new(Sep7Parameters.MemoType, memoType),
            new(Sep7Parameters.Callback, NormalizeCallback(callback)),
            new(Sep7Parameters.Msg, message),
            new(Sep7Parameters.NetworkPassphrase, networkPassphrase),
            new(Sep7Parameters.OriginDomain, originDomain),
        };
        return BuildUri(OperationPay, parameters);
    }

    /// <summary>
    ///     Parses and validates a SEP-7 URI. It checks:
    ///     <list type="bullet">
    ///         <item>the <c>web+stellar:</c> scheme, a <c>tx</c> or <c>pay</c> operation, no fragment;</item>
    ///         <item>well-formed, non-repeated query parameters; known parameters are non-empty;</item>
    ///         <item>
    ///             no parameter of the other operation (<c>xdr</c>/<c>replace</c>/<c>pubkey</c>/<c>chain</c> are
    ///             <c>tx</c>-only; <c>destination</c>/<c>amount</c>/<c>asset_*</c>/<c>memo*</c> are <c>pay</c>-only);
    ///         </item>
    ///         <item>
    ///             <c>tx</c>: <c>xdr</c> decodes as a transaction envelope, <c>pubkey</c> is an account id,
    ///             <c>replace</c> follows the SEP-7 grammar with balanced identifiers, <c>chain</c> is itself a
    ///             valid SEP-7 URI nested at most 7 levels;
    ///         </item>
    ///         <item>
    ///             <c>pay</c>: <c>destination</c> is a G/M/C address or a federation address (<c>name*domain</c>), <c>amount</c> is a positive amount with at most
    ///             7 decimals, <c>asset_code</c> (1–12 alphanumerics) and <c>asset_issuer</c> (G...) come together,
    ///             <c>memo_type</c> is a SEP-7 memo type and <c>memo</c> fits it (hash memos exactly 32 bytes);
    ///             account ids and other strkeys must be upper case;
    ///         </item>
    ///         <item>
    ///             <c>callback</c> is <c>url:</c> plus an absolute http(s) URL without whitespace or credentials (SEP-7
    ///             allows http; <see cref="SubmitToCallbackAsync" /> still refuses it except for loopback addresses),
    ///             <c>msg</c> is at most 300
    ///             characters, <c>origin_domain</c> is a fully qualified domain name, <c>signature</c> is 64
    ///             base64-encoded bytes.
    ///         </item>
    ///     </list>
    ///     A request with an <c>origin_domain</c> but no <c>signature</c> is invalid, as SEP-7 specifies. Query values
    ///     are decoded like an HTML form and the other Stellar SDKs do: <c>+</c> is a space, except in the base64
    ///     values (<c>xdr</c>, <c>signature</c>, hash memos), where it stays a <c>+</c>. The request signature is not
    ///     verified here; see <see cref="VerifySignature" /> and <see cref="VerifyOriginDomainSignatureAsync" />.
    /// </summary>
    /// <param name="uri">The URI to parse.</param>
    /// <returns>The parsed request.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri" /> is null.</exception>
    /// <exception cref="InvalidSep7UriException">Thrown when the URI is invalid; the message says why.</exception>
    /// <exception cref="InvalidOriginDomainException">Thrown when <c>origin_domain</c> is not a fully qualified domain name.</exception>
    public static Sep7Uri ParseUri(string uri)
    {
        Throw.IfNull(uri, nameof(uri));
        return Sep7UriParser.Parse(uri, 0, true);
    }

    /// <summary>Parses and validates a SEP-7 URI without throwing. See <see cref="ParseUri" /> for the checks.</summary>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="result">The parsed request, or <c>null</c> when the URI is invalid.</param>
    /// <returns><c>true</c> when the URI is valid.</returns>
    public static bool TryParseUri(string? uri, out Sep7Uri? result)
    {
        result = null;
        if (uri == null)
        {
            return false;
        }
        try
        {
            result = Sep7UriParser.Parse(uri, 0, true);
            return true;
        }
        catch (InvalidSep7UriException)
        {
            return false;
        }
    }

    /// <summary>Validates a SEP-7 URI and reports why it is invalid. See <see cref="ParseUri" /> for the checks.</summary>
    /// <param name="uri">The URI to validate.</param>
    /// <returns>The validation outcome, with the reason when invalid.</returns>
    public static Sep7ValidationResult ValidateUri(string? uri)
    {
        if (uri == null)
        {
            return Sep7ValidationResult.Invalid("The URI is null.");
        }
        try
        {
            Sep7UriParser.Parse(uri, 0, true);
            return Sep7ValidationResult.Valid();
        }
        catch (InvalidSep7UriException ex)
        {
            return Sep7ValidationResult.Invalid(ex);
        }
    }

    /// <summary>
    ///     Parses a URL-decoded <c>replace</c> value, e.g.
    ///     <c>sourceAccount:X,operations[0].sourceAccount:Y;X:account paying fees,Y:receiving account</c>, into one
    ///     <see cref="Sep7Replacement" /> per field, in order. An empty string gives an empty list.
    /// </summary>
    /// <param name="replace">The <c>replace</c> value.</param>
    /// <returns>The replacements.</returns>
    /// <exception cref="InvalidSep7UriException">
    ///     Thrown when the value does not follow the grammar, repeats a field or hint, uses a path SEP-7 forbids
    ///     (<c>tx.</c> prefix, signatures, <c>_present</c>/<c>len</c>), or its identifiers are unbalanced.
    /// </exception>
    public static IReadOnlyList<Sep7Replacement> ParseReplacements(string replace)
    {
        Throw.IfNull(replace, nameof(replace));
        return Sep7UriParser.ParseReplacements(replace);
    }

    /// <summary>
    ///     Builds a <c>replace</c> value (not yet URL-encoded) from replacements: <c>path:id</c> entries joined
    ///     with <c>,</c>, then <c>;</c>, then one <c>id:hint</c> per identifier in first-seen order.
    /// </summary>
    /// <param name="replacements">The replacements; fields sharing an identifier must share its hint.</param>
    /// <returns>The <c>replace</c> value; empty when there are no replacements.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown for an invalid path, an empty or delimiter-containing identifier or hint, a repeated path, or
    ///     one identifier with two different hints.
    /// </exception>
    public static string ReplacementsToString(IEnumerable<Sep7Replacement> replacements)
    {
        Throw.IfNull(replacements, nameof(replacements));

        var fields = new List<string>();
        var hints = new List<KeyValuePair<string, string>>();
        var hintById = new Dictionary<string, string>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var replacement in replacements)
        {
            Throw.IfNull(replacement, nameof(replacements));
            var pathError = Sep7UriParser.ValidateTxrepPath(replacement.Path);
            if (pathError != null)
            {
                throw new ArgumentException(pathError, nameof(replacements));
            }
            if (!paths.Add(replacement.Path))
            {
                throw new ArgumentException(
                    $"The field '{replacement.Path}' is listed more than once.", nameof(replacements));
            }
            if (replacement.Id.Length == 0 ||
                replacement.Id.IndexOfAny(new[] { ReplaceIdDelimiter, ReplaceListDelimiter, ReplaceHintDelimiter }) >= 0)
            {
                throw new ArgumentException(
                    $"The reference identifier '{replacement.Id}' must be non-empty and must not contain ':', ',' or ';'.",
                    nameof(replacements));
            }
            // Hints may not contain ':' either: this parser accepts it, but other SDKs cut the hint off there.
            if (replacement.Hint.Length == 0 ||
                replacement.Hint.IndexOfAny(new[] { ReplaceIdDelimiter, ReplaceListDelimiter, ReplaceHintDelimiter }) >= 0)
            {
                throw new ArgumentException(
                    $"The hint for '{replacement.Id}' must be non-empty and must not contain ':', ',' or ';'.",
                    nameof(replacements));
            }
            if (hintById.TryGetValue(replacement.Id, out var existingHint))
            {
                if (existingHint != replacement.Hint)
                {
                    throw new ArgumentException(
                        $"The reference identifier '{replacement.Id}' has two different hints.", nameof(replacements));
                }
            }
            else
            {
                hintById.Add(replacement.Id, replacement.Hint);
                hints.Add(new KeyValuePair<string, string>(replacement.Id, replacement.Hint));
            }
            fields.Add(replacement.Path + ReplaceIdDelimiter + replacement.Id);
        }

        if (fields.Count == 0)
        {
            return string.Empty;
        }
        return string.Join(ReplaceListDelimiter.ToString(), fields) + ReplaceHintDelimiter +
               string.Join(ReplaceListDelimiter.ToString(), hints.Select(h => h.Key + ReplaceIdDelimiter + h.Value));
    }

    /// <summary>
    ///     Signs a SEP-7 URI with the origin domain's <c>URI_REQUEST_SIGNING_KEY</c> and appends the result as the
    ///     last parameter, <c>&amp;signature=</c>, base64- and then URL-encoded. The signature covers the URI string
    ///     exactly as given, so do not re-encode the returned URI.
    /// </summary>
    /// <param name="uri">The unsigned URI; it must carry an <c>origin_domain</c>.</param>
    /// <param name="signer">The key pair matching the domain's <c>URI_REQUEST_SIGNING_KEY</c>.</param>
    /// <returns>The signed URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri" /> or <paramref name="signer" /> is null.</exception>
    /// <exception cref="InvalidSep7UriException">Thrown when the URI is invalid or already signed.</exception>
    /// <exception cref="MissingOriginDomainException">Thrown when the URI has no <c>origin_domain</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="signer" /> has no private key.</exception>
    public static string SignUri(string uri, KeyPair signer)
    {
        Throw.IfNull(uri, nameof(uri));
        Throw.IfNull(signer, nameof(signer));
        // The URI being signed is the requester's own: it has an origin_domain and, until now, no signature.
        var request = Sep7UriParser.Parse(uri, 0, false);
        if (request.Signature != null)
        {
            throw new InvalidSep7UriException("The URI already has a 'signature' parameter.");
        }
        if (request.OriginDomain == null)
        {
            throw new MissingOriginDomainException();
        }
        if (!signer.CanSign())
        {
            throw new ArgumentException("The signer key pair has no private key.", nameof(signer));
        }

        var signature = Convert.ToBase64String(signer.Sign(BuildSignaturePayload(uri)));
        return uri + "&" + Sep7Parameters.Signature + "=" + Uri.EscapeDataString(signature);
    }

    /// <summary>
    ///     Checks the URI's <c>signature</c> against a known signing key, offline. The payload is the received URI
    ///     string with the <c>signature</c> parameter removed. To check against the origin domain's published key
    ///     use <see cref="VerifyOriginDomainSignatureAsync" />.
    /// </summary>
    /// <param name="uri">The signed URI, exactly as received.</param>
    /// <param name="signerAccountId">The account id (G...) expected to have signed, in either case.</param>
    /// <returns>
    ///     <c>true</c> when the signature verifies; <c>false</c> when it does not, the URI is invalid or unsigned,
    ///     or <paramref name="signerAccountId" /> is not a valid account id.
    /// </returns>
    public static bool VerifySignature(string uri, string signerAccountId)
    {
        Throw.IfNull(uri, nameof(uri));
        Throw.IfNull(signerAccountId, nameof(signerAccountId));

        if (!TryParseUri(uri, out var request) || request!.Signature == null ||
            !StrKey.IsValidEd25519PublicKey(signerAccountId))
        {
            return false;
        }

        var payload = BuildSignaturePayload(RemoveSignatureParameter(uri));
        return KeyPair.FromAccountId(signerAccountId).Verify(payload, Convert.FromBase64String(request.Signature));
    }

    /// <summary>
    ///     Verifies a request's <c>origin_domain</c> as SEP-7 prescribes, before a wallet may show the domain:
    ///     the URI must be valid and carry both <c>origin_domain</c> and <c>signature</c>; the domain's
    ///     stellar.toml is fetched (not cached, as the spec recommends) and must publish a valid
    ///     <c>URI_REQUEST_SIGNING_KEY</c>; and the signature must verify against that key.
    /// </summary>
    /// <param name="uri">The signed URI, exactly as received.</param>
    /// <param name="pinnedSigningKey">
    ///     The <c>URI_REQUEST_SIGNING_KEY</c> the wallet cached for this domain from an earlier request, if any.
    ///     SEP-7 requires wallets to alert the user when the key changes; a different key raises
    ///     <see cref="UriRequestSigningKeyChangedException" />. Cache the returned key for the next request, keyed by
    ///     the domain in lower case (domain names are case-insensitive, and <see cref="Sep7Uri.OriginDomain" /> is
    ///     returned as sent). Keys are compared case-insensitively.
    /// </param>
    /// <param name="cancellationToken">Cancels the stellar.toml fetch.</param>
    /// <returns>The <c>URI_REQUEST_SIGNING_KEY</c> the signature verified against.</returns>
    /// <remarks>
    ///     Only the <c>URI_REQUEST_SIGNING_KEY</c> of the stellar.toml's root table is read, with a non-recursive
    ///     reader rather than the general <see cref="StellarDotnetSdk.Sep.Sep0001.StellarToml" /> parser: the domain
    ///     is chosen by whoever wrote the URI, and that parser overflows the stack on small hostile documents.
    ///     Redirects of the stellar.toml request that reach this class are followed, at most 5 of them and only to
    ///     https URLs. With the SDK's own client that is every redirect; a client that follows redirects itself
    ///     applies its own limits first (see the constructor).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri" /> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance or its HTTP client has been disposed.</exception>
    /// <exception cref="InvalidSep7UriException">Thrown when the URI is invalid.</exception>
    /// <exception cref="InvalidOriginDomainException">Thrown when <c>origin_domain</c> is not a fully qualified domain name.</exception>
    /// <exception cref="MissingOriginDomainException">Thrown when the URI has no <c>origin_domain</c>.</exception>
    /// <exception cref="MissingSignatureException">Thrown when the URI has no <c>signature</c>.</exception>
    /// <exception cref="OriginDomainStellarTomlException">
    ///     Thrown when the stellar.toml cannot be fetched (including a connection dropped mid-response, a response
    ///     over 512 KiB, a redirect that is not https, and a request the client's resilience pipeline rejected or
    ///     timed out) or its root table cannot be read.
    /// </exception>
    /// <exception cref="NoUriRequestSigningKeyFoundException">Thrown when the stellar.toml has no <c>URI_REQUEST_SIGNING_KEY</c>.</exception>
    /// <exception cref="InvalidUriRequestSigningKeyException">Thrown when the published key is not a valid account id.</exception>
    /// <exception cref="UriRequestSigningKeyChangedException">Thrown when the published key differs from <paramref name="pinnedSigningKey" />.</exception>
    /// <exception cref="InvalidSep7SignatureException">Thrown when the signature does not verify.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is cancelled or the fetch, response body included, takes
    ///     longer than the HTTP client's <see cref="System.Net.Http.HttpClient.Timeout" />.
    /// </exception>
    public async Task<string> VerifyOriginDomainSignatureAsync(
        string uri,
        string? pinnedSigningKey = null,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(uri, nameof(uri));
        // Parsed without the signed-origin_domain rule so that its absence gets its own exception type.
        var request = Sep7UriParser.Parse(uri, 0, false);
        var domain = request.OriginDomain ?? throw new MissingOriginDomainException();
        if (request.Signature == null)
        {
            throw new MissingSignatureException();
        }

        var signingKey = await FetchUriRequestSigningKeyAsync(domain, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(signingKey))
        {
            throw new NoUriRequestSigningKeyFoundException(domain);
        }
        if (!StrKey.IsValidEd25519PublicKey(signingKey!))
        {
            throw new InvalidUriRequestSigningKeyException(domain, signingKey!);
        }
        // Strkeys are case-insensitive base32, so a key pinned in another case is the same key.
        if (pinnedSigningKey != null && !string.Equals(pinnedSigningKey, signingKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new UriRequestSigningKeyChangedException(domain, pinnedSigningKey, signingKey!);
        }
        if (!VerifySignature(uri, signingKey!))
        {
            throw new InvalidSep7SignatureException(domain, signingKey!);
        }
        return signingKey!;
    }

    /// <summary>
    ///     Runs <see cref="VerifyOriginDomainSignatureAsync" /> and reports the outcome instead of throwing: every
    ///     way the URI, the stellar.toml or its fetch can be wrong is reported as invalid, a <c>null</c> URI
    ///     included. Only cancellation, a timeout and using a disposed instance still throw.
    /// </summary>
    /// <param name="uri">The signed URI, exactly as received.</param>
    /// <param name="pinnedSigningKey">The key the wallet cached for this domain earlier, if any.</param>
    /// <param name="cancellationToken">Cancels the stellar.toml fetch.</param>
    /// <returns>Valid when the origin domain's signature verifies; otherwise invalid with the reason.</returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is cancelled or the fetch, response body included, takes
    ///     longer than the HTTP client's <see cref="System.Net.Http.HttpClient.Timeout" />.
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance or its HTTP client has been disposed.</exception>
    public async Task<Sep7ValidationResult> IsValidSignedUriAsync(
        string? uri,
        string? pinnedSigningKey = null,
        CancellationToken cancellationToken = default)
    {
        if (uri == null)
        {
            return Sep7ValidationResult.Invalid("The URI is null.");
        }
        try
        {
            await VerifyOriginDomainSignatureAsync(uri, pinnedSigningKey, cancellationToken).ConfigureAwait(false);
            return Sep7ValidationResult.Valid();
        }
        catch (Sep7Exception ex)
        {
            return Sep7ValidationResult.Invalid(ex);
        }
    }

    /// <summary>
    ///     Signs the transaction of a <c>tx</c> request for the request's network and hands it on: POSTed to the
    ///     <c>callback</c> URL when there is one, otherwise submitted to <paramref name="horizonServer" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Show the request to the user and get their approval first. A request with a <c>replace</c> parameter
    ///         must have its fields filled in before signing: verify any <c>origin_domain</c> with
    ///         <see cref="VerifyOriginDomainSignatureAsync" />, take <see cref="Sep7Uri.GetTransaction" />, apply the
    ///         replacements, sign, and call <see cref="SubmitTransactionAsync" />.
    ///     </para>
    ///     <para>
    ///         A request with an <c>origin_domain</c> is signed only after its signature verifies against the
    ///         domain's current <c>URI_REQUEST_SIGNING_KEY</c>, as SEP-7 requires: this method runs
    ///         <see cref="VerifyOriginDomainSignatureAsync" /> itself, fetching the stellar.toml again. Pass the key
    ///         that verified when the request was shown to the user as <paramref name="pinnedSigningKey" /> so that
    ///         a key change in between is caught too.
    ///     </para>
    ///     <para>
    ///         The transaction is signed for the request's network: its <c>network_passphrase</c>, or the public
    ///         network when it has none, as SEP-7 specifies. The requester chooses that value, so show
    ///         <see cref="Sep7Uri.GetNetwork" /> to the user, or compare it with the network the wallet is on, before
    ///         approving; a request without a passphrase asks for a public-network signature. When the request has
    ///         no callback, pass a <paramref name="horizonServer" /> for that same network; this method cannot check
    ///         which network a <see cref="Server" /> serves.
    ///     </para>
    /// </remarks>
    /// <param name="uri">The <c>tx</c> request.</param>
    /// <param name="signer">The key pair to sign with; must match the request's <c>pubkey</c> when present.</param>
    /// <param name="horizonServer">The Horizon server to submit to when the request has no callback.</param>
    /// <param name="pinnedSigningKey">
    ///     The <c>URI_REQUEST_SIGNING_KEY</c> the wallet expects for the request's <c>origin_domain</c>, if any; see
    ///     <see cref="VerifyOriginDomainSignatureAsync" />. Passing one for a request without an
    ///     <c>origin_domain</c> raises <see cref="MissingOriginDomainException" />: the key cannot be checked.
    /// </param>
    /// <param name="cancellationToken">Cancels the stellar.toml fetch and the callback POST.</param>
    /// <returns>
    ///     The callback's or Horizon's answer. Neither a callback's non-2xx status nor a transaction Horizon rejects
    ///     is thrown; check <see cref="Sep7SubmitResult.IsSuccess" />.
    /// </returns>
    /// <exception cref="InvalidSep7UriException">
    ///     Thrown when the URI is invalid, including an <c>origin_domain</c> without a <c>signature</c>.
    /// </exception>
    /// <exception cref="Sep7Exception">
    ///     Thrown when the request's <c>origin_domain</c> signature cannot be verified; the subtypes are those listed
    ///     on <see cref="VerifyOriginDomainSignatureAsync" />. Nothing is signed or sent in that case.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown for a <c>pay</c> request, a signer without a private key or that does not match <c>pubkey</c>, or
    ///     a callback that is not https (plain http is allowed only for loopback addresses).
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="uri" /> or <paramref name="signer" /> is null, or when the request has no
    ///     callback and no Horizon server is given.
    /// </exception>
    /// <exception cref="StellarDotnetSdk.Exceptions.AccountRequiresMemoException">
    ///     Thrown by the Horizon path when a destination requires a memo (SEP-29); the other exceptions of
    ///     <see cref="Server.SubmitTransaction(Transaction)" /> apply there too.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the request has a <c>replace</c> parameter.</exception>
    /// <exception cref="HttpRequestException">
    ///     Thrown when the callback POST fails, is redirected, or its response exceeds 512 KiB; see
    ///     <see cref="SubmitToCallbackAsync" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is cancelled or an HTTP exchange takes longer than the
    ///     HTTP client's <see cref="System.Net.Http.HttpClient.Timeout" />.
    /// </exception>
    public async Task<Sep7SubmitResult> SignAndSubmitTransactionAsync(
        string uri,
        KeyPair signer,
        Server? horizonServer = null,
        string? pinnedSigningKey = null,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(uri, nameof(uri));
        Throw.IfNull(signer, nameof(signer));
        if (!signer.CanSign())
        {
            throw new ArgumentException("The signer key pair has no private key.", nameof(signer));
        }
        var request = ParseUri(uri);
        if (request.OperationType != Sep7OperationType.Tx)
        {
            throw new ArgumentException(
                "Only 'tx' requests carry a transaction to sign; build the payment a 'pay' request asks for and " +
                "submit it yourself.", nameof(uri));
        }
        if (request.Replacements.Count > 0)
        {
            throw new InvalidOperationException(
                "The request asks for transaction fields to be filled in ('replace'). Apply them to the transaction " +
                "from Sep7Uri.GetTransaction(), sign it, and pass it to SubmitTransactionAsync.");
        }
        if (request.PublicKey != null && request.PublicKey != signer.AccountId)
        {
            throw new ArgumentException(
                $"The request asks for a signature from '{request.PublicKey}', not '{signer.AccountId}'.",
                nameof(signer));
        }
        if (pinnedSigningKey != null && request.OriginDomain == null)
        {
            throw new MissingOriginDomainException();
        }
        // Local checks on where the transaction will go come before the stellar.toml round trip.
        if (request.CallbackUrl != null)
        {
            ResolveCallbackTarget(request.CallbackUrl);
        }
        else if (horizonServer == null)
        {
            throw new ArgumentNullException(nameof(horizonServer),
                "The request has no callback, so the signed transaction goes to the network; pass a Server for " +
                "the request's network.");
        }
        if (request.OriginDomain != null)
        {
            await VerifyOriginDomainSignatureAsync(uri, pinnedSigningKey, cancellationToken).ConfigureAwait(false);
        }

        var transaction = request.GetTransaction();
        transaction.Sign(signer, request.GetNetwork());
        return await SubmitTransactionAsync(request, transaction, horizonServer, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Hands a signed transaction for a request on: POSTed to the request's <c>callback</c> URL when there is
    ///     one, otherwise submitted to <paramref name="horizonServer" />. For a <c>tx</c> request that is its
    ///     transaction with any replacements applied; for a <c>pay</c> request, the payment transaction the wallet
    ///     built from it (SEP-7 defines <c>callback</c> for both operations).
    /// </summary>
    /// <remarks>
    ///     Nothing is verified here: when the request has an <c>origin_domain</c>, run
    ///     <see cref="VerifyOriginDomainSignatureAsync" /> before signing, as SEP-7 requires.
    /// </remarks>
    /// <param name="request">The parsed request.</param>
    /// <param name="signedTransaction">
    ///     The transaction to hand on, with at least one signature.
    /// </param>
    /// <param name="horizonServer">The Horizon server to submit to when the request has no callback.</param>
    /// <param name="cancellationToken">Cancels the callback POST; the Horizon submission does not take a token.</param>
    /// <returns>
    ///     The callback's or Horizon's answer. A callback that answers with a non-2xx status is returned, not thrown;
    ///     check <see cref="Sep7SubmitResult.IsSuccess" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when an argument is null, or when the request has no callback and no Horizon server is given.
    /// </exception>
    /// <exception cref="StellarDotnetSdk.Exceptions.NotEnoughSignaturesException">Thrown when the transaction has no signature.</exception>
    /// <exception cref="StellarDotnetSdk.Exceptions.AccountRequiresMemoException">
    ///     Thrown by the Horizon path when a destination requires a memo (SEP-29) and the transaction has none;
    ///     the other exceptions of <see cref="Server.SubmitTransaction(Transaction)" /> apply there too.
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance or its HTTP client has been disposed.</exception>
    /// <exception cref="ArgumentException">Thrown when the callback is not https (http is allowed only for loopback).</exception>
    /// <exception cref="HttpRequestException">
    ///     Thrown when the callback POST fails, is redirected, or its response exceeds 512 KiB; see
    ///     <see cref="SubmitToCallbackAsync" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is cancelled or the callback POST, response body
    ///     included, takes longer than the HTTP client's <see cref="System.Net.Http.HttpClient.Timeout" />.
    /// </exception>
    public async Task<Sep7SubmitResult> SubmitTransactionAsync(
        Sep7Uri request,
        TransactionBase signedTransaction,
        Server? horizonServer = null,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(request, nameof(request));
        Throw.IfNull(signedTransaction, nameof(signedTransaction));

        var envelopeXdr = signedTransaction.ToEnvelopeXdrBase64();
        if (request.CallbackUrl != null)
        {
            return await SubmitToCallbackAsync(request.CallbackUrl, envelopeXdr, cancellationToken)
                .ConfigureAwait(false);
        }
        if (horizonServer == null)
        {
            throw new ArgumentNullException(nameof(horizonServer),
                "The request has no callback, so the signed transaction goes to the network; pass a Server for " +
                "the request's network.");
        }
        // The typed overloads tell Server a fee-bump envelope is one; the string overload would try to parse it as a
        // plain transaction and throw. Both run the SEP-29 memo-required check.
        var response = signedTransaction switch
        {
            FeeBumpTransaction feeBump => await horizonServer.SubmitTransaction(feeBump).ConfigureAwait(false),
            Transaction transaction => await horizonServer.SubmitTransaction(transaction).ConfigureAwait(false),
            _ => await horizonServer.SubmitTransaction(envelopeXdr).ConfigureAwait(false),
        };
        return new Sep7SubmitResult(response);
    }

    /// <summary>
    ///     POSTs a signed transaction to a SEP-7 callback URL as <c>application/x-www-form-urlencoded</c> with a
    ///     single <c>xdr</c> field, as SEP-7 specifies. Query parameters already on the URL are kept.
    /// </summary>
    /// <remarks>
    ///     Redirects are not followed: a callback that answers 3xx has its status returned, since following it
    ///     would either drop the transaction (301/302/303 turn the POST into a GET) or send it to a URL that was
    ///     never checked (307/308). If the HTTP client given to the constructor follows redirects by itself, the
    ///     transaction may already have gone to the redirect target; the redirect is then detected afterwards and
    ///     reported as an <see cref="HttpRequestException" />. That detection compares the request's final method
    ///     and URL with the callback's, so it is best effort: a redirect chain that ends back on the callback URL,
    ///     or a handler that clones the request before redirecting, escapes it, and a handler that rewrites the
    ///     request URL without redirecting triggers it.
    ///     <para>
    ///         Any https host is accepted, private and link-local addresses included, as is plain http to any
    ///         loopback port: SEP-7 lets the requester pick the callback. A service that signs requests on behalf of
    ///         others should restrict the hosts it POSTs to before calling this method.
    ///     </para>
    /// </remarks>
    /// <param name="callbackUrl">The callback URL, with or without the <c>url:</c> prefix.</param>
    /// <param name="signedTransactionEnvelopeXdr">The signed base64 XDR transaction envelope.</param>
    /// <param name="cancellationToken">Cancels the POST.</param>
    /// <returns>
    ///     The callback's status code and body (at most 512 KiB). A non-2xx status is returned, not thrown; check
    ///     <see cref="Sep7SubmitResult.IsSuccess" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when the URL is not absolute https; plain http is allowed only for loopback addresses.
    /// </exception>
    /// <exception cref="HttpRequestException">
    ///     Thrown when the POST fails (including a connection dropped mid-response and a request the client's
    ///     resilience pipeline rejected or timed out), when the HTTP client followed a redirect, or when the
    ///     response exceeds 512 KiB.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is cancelled or the POST, response body included, takes
    ///     longer than the HTTP client's <see cref="System.Net.Http.HttpClient.Timeout" />.
    /// </exception>
    public async Task<Sep7SubmitResult> SubmitToCallbackAsync(
        string callbackUrl,
        string signedTransactionEnvelopeXdr,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNullOrEmpty(callbackUrl, nameof(callbackUrl));
        Throw.IfNullOrEmpty(signedTransactionEnvelopeXdr, nameof(signedTransactionEnvelopeXdr));

        var target = ResolveCallbackTarget(callbackUrl);

        using var request = new HttpRequestMessage(HttpMethod.Post, target)
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(Sep7Parameters.Xdr, signedTransactionEnvelopeXdr),
            }),
        };
        using var timeout = CreateExchangeTimeout(cancellationToken);
        HttpStatusCode? answered = null;
        try
        {
            // ResponseHeadersRead: HttpClient would otherwise buffer the whole body before the bounded read runs.
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                .ConfigureAwait(false);
            answered = response.StatusCode;
            // A redirecting handler rewrites the request it followed in place.
            if (request.Method != HttpMethod.Post || request.RequestUri != target)
            {
                throw new HttpRequestException(
                    $"The HTTP client followed a redirect of the callback POST to {Sep7UriParser.Echo(request.RequestUri?.ToString() ?? "")} " +
                    $"({request.Method}), so the answer is not the callback's. Use an HTTP client with automatic " +
                    "redirects turned off.");
            }
            string body;
            try
            {
                body = await ReadBodyBoundedAsync(response, timeout.Token).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                // Over the size cap: the callback did answer, so it has the transaction.
                throw new HttpRequestException(
                    $"POSTing to the callback {Sep7UriParser.Echo(target.ToString())} failed after the callback answered HTTP " +
                    $"{(int)response.StatusCode}: {ex.Message}", ex);
            }
            return new Sep7SubmitResult(response.StatusCode, body);
        }
        catch (OperationCanceledException ex) when (IsExchangeTimeout(timeout, cancellationToken))
        {
            throw ExchangeTimedOut(target, ex, answered);
        }
        catch (Exception ex) when (ex is IOException or ExecutionRejectedException or UriFormatException)
        {
            // Once the status line has arrived the callback has the transaction; say so, since a caller that
            // retries on HttpRequestException would otherwise send it again. A UriFormatException comes from a
            // client that followed the callback's redirect, which means the callback answered too.
            var afterAnswer = answered is { } status ? $" after the callback answered HTTP {(int)status}"
                : ex is UriFormatException ? " after the callback answered with a redirect" : "";
            throw new HttpRequestException($"POSTing to the callback {Sep7UriParser.Echo(target.ToString())} failed{afterAnswer}: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    ///     Builds the bytes a request signature covers: 35 zero bytes, a byte <c>4</c>, then the UTF-8 bytes of
    ///     <see cref="SignaturePayloadPrefix" /> followed by the URI (without its <c>signature</c> parameter).
    /// </summary>
    internal static byte[] BuildSignaturePayload(string uriWithoutSignature)
    {
        var data = Encoding.UTF8.GetBytes(SignaturePayloadPrefix + uriWithoutSignature);
        var payload = new byte[36 + data.Length];
        payload[35] = 4;
        Buffer.BlockCopy(data, 0, payload, 36, data.Length);
        return payload;
    }

    /// <summary>
    ///     Removes the <c>signature</c> parameter from a URI, leaving every other character as received. SEP-7
    ///     puts the signature last, but it is removed wherever it is.
    /// </summary>
    internal static string RemoveSignatureParameter(string uri)
    {
        var queryStart = uri.IndexOf('?');
        if (queryStart < 0)
        {
            return uri;
        }
        var kept = uri.Substring(queryStart + 1).Split('&').Where(segment =>
        {
            var separator = segment.IndexOf('=');
            var name = separator < 0 ? segment : segment.Substring(0, separator);
            return Uri.UnescapeDataString(name) != Sep7Parameters.Signature;
        });
        return uri.Substring(0, queryStart + 1) + string.Join("&", kept);
    }

    private static string BuildUri(string operation, IEnumerable<KeyValuePair<string, string?>> parameters)
    {
        parameters = parameters.ToList();
        // An empty msg or amount is omitted. An empty value for a parameter whose absence changes where the
        // transaction goes, who may sign it or which network it is for is rejected instead, since omitting it
        // would silently turn the request into a different one (no callback = submit to Horizon; no pubkey = any
        // signer; no network_passphrase = the public network; no origin_domain = an unattributed request).
        foreach (var (name, argument) in EmptyMeansSomethingElse)
        {
            if (parameters.Any(p => p.Key == name && p.Value?.Length == 0))
            {
                throw new ArgumentException(
                    $"The {argument} must not be empty; pass null to leave the '{name}' parameter out.", argument);
            }
        }
        var query = string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => p.Key + "=" + Uri.EscapeDataString(p.Value!)));
        var uri = SchemePrefix + operation + "?" + query;
        try
        {
            // An origin_domain is signed later, with SignUri.
            Sep7UriParser.Parse(uri, 0, false);
        }
        catch (InvalidSep7UriException ex)
        {
            throw new ArgumentException(ex.Message, ex);
        }
        return uri;
    }

    /// <summary>
    ///     The URL a callback POST goes to: <paramref name="callbackUrl" /> without its <c>url:</c> prefix, which must
    ///     be absolute https, or plain http to a loopback address.
    /// </summary>
    private static Uri ResolveCallbackTarget(string callbackUrl)
    {
        var url = callbackUrl.StartsWith(CallbackUrlPrefix, StringComparison.Ordinal)
            ? callbackUrl.Substring(CallbackUrlPrefix.Length)
            : callbackUrl;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var target) ||
            !(target.Scheme == Uri.UriSchemeHttps || (target.Scheme == Uri.UriSchemeHttp && target.IsLoopback)))
        {
            throw new ArgumentException(
                "The callback must be an absolute https URL (it receives the signed transaction); plain http is " +
                "allowed only for loopback addresses used in local development.", nameof(callbackUrl));
        }
        return target;
    }

    private static readonly (string Name, string Argument)[] EmptyMeansSomethingElse =
    {
        (Sep7Parameters.Callback, "callback"),
        (Sep7Parameters.Pubkey, "publicKey"),
        (Sep7Parameters.NetworkPassphrase, "networkPassphrase"),
        (Sep7Parameters.OriginDomain, "originDomain"),
    };

    private static string? NormalizeCallback(string? callback)
    {
        if (callback == null || callback.Length == 0)
        {
            // Returned as-is so BuildUri can reject an empty callback instead of dropping it.
            return callback;
        }
        return callback!.StartsWith(CallbackUrlPrefix, StringComparison.Ordinal)
            ? callback
            : CallbackUrlPrefix + callback;
    }

    private static (string? Value, string? Type) EncodeMemo(Memo? memo)
    {
        return memo switch
        {
            null or MemoNone => (null, null),
            MemoText { MemoTextValue.Length: 0 } => throw new ArgumentException(
                "An empty text memo cannot be expressed in SEP-7 (parameters must not be empty); pass null or " +
                "MemoNone for no memo.", nameof(memo)),
            MemoText text => (text.MemoTextValue, Sep7MemoType.MemoText),
            MemoId id => (id.IdValue.ToString(CultureInfo.InvariantCulture), Sep7MemoType.MemoId),
            MemoReturnHash returnHash => (Convert.ToBase64String(returnHash.MemoBytes), Sep7MemoType.MemoReturn),
            MemoHash hash => (Convert.ToBase64String(hash.MemoBytes), Sep7MemoType.MemoHash),
            _ => throw new ArgumentException($"Unsupported memo type {memo.GetType().Name}.", nameof(memo)),
        };
    }

    /// <summary>
    ///     Fetches the origin domain's stellar.toml and returns its root <c>URI_REQUEST_SIGNING_KEY</c>, or
    ///     <c>null</c> when it has none. Transport and format failures become
    ///     <see cref="OriginDomainStellarTomlException" />.
    /// </summary>
    private async Task<string?> FetchUriRequestSigningKeyAsync(string domain, CancellationToken cancellationToken)
    {
        // The domain passed IsFullyQualifiedDomainName, so it cannot smuggle a port, path or userinfo into the URL.
        var tomlUri = new Uri($"https://{domain}/.well-known/stellar.toml");
        string body;
        using (var timeout = CreateExchangeTimeout(cancellationToken))
        {
            try
            {
                body = await GetStellarTomlBodyAsync(domain, tomlUri, timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (IsExchangeTimeout(timeout, cancellationToken))
            {
                throw ExchangeTimedOut(tomlUri, ex, null);
            }
            // UriFormatException: an HTTP client that follows redirects itself throws it for a malformed Location.
            catch (Exception ex) when (ex is HttpRequestException or IOException or ExecutionRejectedException
                                           or UriFormatException)
            {
                throw new OriginDomainStellarTomlException(domain, $"Fetching {tomlUri} failed: {ex.Message}", ex);
            }
        }

        try
        {
            return Sep7StellarTomlReader.ReadUriRequestSigningKey(body);
        }
        catch (FormatException ex)
        {
            throw new OriginDomainStellarTomlException(domain, $"The stellar.toml could not be parsed: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    ///     GETs the stellar.toml, following up to <see cref="MaxStellarTomlRedirects" /> redirects itself (the
    ///     SDK's own client does not follow any), each to an https URL only.
    /// </summary>
    private async Task<string> GetStellarTomlBodyAsync(string domain, Uri tomlUri,
        CancellationToken cancellationToken)
    {
        var target = tomlUri;
        for (var redirects = 0;; redirects++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, target);
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            if (IsRedirect(response))
            {
                // Location is null when the header is missing or unparseable; a relative one that cannot be
                // resolved against the current URL is rejected by TryCreate instead of throwing.
                if (response.Headers.Location is not { } location || !Uri.TryCreate(target, location, out var next))
                {
                    throw new OriginDomainStellarTomlException(domain,
                        $"{Sep7UriParser.Echo(target.ToString())} answered with HTTP status {(int)response.StatusCode} " +
                        "but no usable Location header.");
                }
                if (next.Scheme != Uri.UriSchemeHttps)
                {
                    throw new OriginDomainStellarTomlException(domain,
                        $"{Sep7UriParser.Echo(target.ToString())} redirected to {Sep7UriParser.Echo(next.ToString())}, " +
                        "which is not https.");
                }
                if (redirects >= MaxStellarTomlRedirects)
                {
                    throw new OriginDomainStellarTomlException(domain,
                        $"{tomlUri} redirected more than {MaxStellarTomlRedirects} times.");
                }
                target = next;
                continue;
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new OriginDomainStellarTomlException(domain,
                    $"{Sep7UriParser.Echo(target.ToString())} answered with HTTP status {(int)response.StatusCode}.");
            }
            return await ReadBodyBoundedAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsRedirect(HttpResponseMessage response)
    {
        return (int)response.StatusCode is 301 or 302 or 303 or 307 or 308;
    }

    /// <summary>
    ///     A token that fires after the HTTP client's <see cref="System.Net.Http.HttpClient.Timeout" />. The requests
    ///     here read their bodies after <see cref="HttpCompletionOption.ResponseHeadersRead" />, which leaves the body
    ///     outside the client's own timeout, so without this a server trickling bytes holds the call open forever.
    /// </summary>
    private CancellationTokenSource CreateExchangeTimeout(CancellationToken cancellationToken)
    {
        var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_httpClient.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
        {
            timeout.CancelAfter(_httpClient.Timeout);
        }
        return timeout;
    }

    private static bool IsExchangeTimeout(CancellationTokenSource timeout, CancellationToken cancellationToken)
    {
        return timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
    }

    private TaskCanceledException ExchangeTimedOut(Uri target, OperationCanceledException ex, HttpStatusCode? answered)
    {
        // The same shape HttpClient uses for its own timeout: a TaskCanceledException around a TimeoutException.
        var afterAnswer = answered is { } status ? $" (it had answered HTTP {(int)status})" : "";
        var message = $"The request to {Sep7UriParser.Echo(target.ToString())} did not complete within the HTTP client's " +
                      $"{_httpClient.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)}-second timeout" +
                      $"{afterAnswer}.";
        return new TaskCanceledException(message, new TimeoutException(message, ex));
    }

    private static HttpMessageHandler CreateNonRedirectingHandler()
    {
#if NET8_0_OR_GREATER
        return new SocketsHttpHandler { AllowAutoRedirect = false };
#else
        return new HttpClientHandler { AllowAutoRedirect = false };
#endif
    }

    /// <summary>
    ///     Reads an HTTP response body into a string with an upper size bound. Throws
    ///     <see cref="HttpRequestException" /> when the body exceeds <see cref="MaxResponseBodyBytes" />.
    /// </summary>
    private static async Task<string> ReadBodyBoundedAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength is long contentLength &&
            contentLength > MaxResponseBodyBytes)
        {
            throw new HttpRequestException(
                $"Response body length {contentLength} exceeds the {MaxResponseBodyBytes}-byte limit.");
        }

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > MaxResponseBodyBytes)
            {
                throw new HttpRequestException($"Response body exceeds the {MaxResponseBodyBytes}-byte limit.");
            }
            buffer.Write(chunk, 0, read);
        }

        return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
    }
}
