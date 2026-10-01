using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Sep.Sep0007.Exceptions;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>Parses and validates SEP-7 URIs and their <c>replace</c> values.</summary>
internal static class Sep7UriParser
{
    // Parameters that belong to one operation only; sending one with the other operation is rejected.
    private static readonly HashSet<string> TxOnlyParameters = new(StringComparer.Ordinal)
    {
        Sep7Parameters.Xdr, Sep7Parameters.Replace, Sep7Parameters.Pubkey, Sep7Parameters.Chain,
    };

    private static readonly HashSet<string> PayOnlyParameters = new(StringComparer.Ordinal)
    {
        Sep7Parameters.Destination, Sep7Parameters.Amount, Sep7Parameters.AssetCode,
        Sep7Parameters.AssetIssuer, Sep7Parameters.Memo, Sep7Parameters.MemoType,
    };

    private static readonly HashSet<string> KnownParameters = new(
        TxOnlyParameters.Concat(PayOnlyParameters).Concat(new[]
        {
            Sep7Parameters.Callback, Sep7Parameters.Msg, Sep7Parameters.NetworkPassphrase,
            Sep7Parameters.OriginDomain, Sep7Parameters.Signature,
        }),
        StringComparer.Ordinal);

    // A fully qualified domain name: dot-separated labels of letters, digits and inner hyphens, ending in an
    // alphabetic or punycode ("xn--") TLD, at most 253 characters. IP addresses, "localhost", ports, paths, userinfo and trailing
    // dots are rejected, so the value can be dropped into https://<domain>/.well-known/stellar.toml safely.
    // \z rather than $, which would also match before a trailing newline.
    private static readonly Regex FqdnRegex = new(
        @"^(?=.{1,253}\z)(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+(?:[a-zA-Z]{2,63}|[xX][nN]--(?:[a-zA-Z0-9-]{0,58}[a-zA-Z0-9]))\z",
        RegexOptions.CultureInvariant);

    // Syntax and scale only: the value range is MaxAmount's, so leading zeros ("0000000000001") are fine.
    private static readonly Regex AmountRegex = new(@"^[0-9]+(?:\.[0-9]{1,7})?\z", RegexOptions.CultureInvariant);

    private static readonly Regex AssetCodeRegex = new(@"^[a-zA-Z0-9]{1,12}\z", RegexOptions.CultureInvariant);

    private static readonly Regex Base64Regex = new(
        @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?\z",
        RegexOptions.CultureInvariant);

    // A SEP-11 Txrep field path: dot-separated identifiers, each optionally indexed, e.g.
    // operations[0].body.paymentOp.destination.
    private static readonly Regex TxrepPathRegex = new(
        @"^[A-Za-z_][A-Za-z0-9_]*(?:\[[0-9]+\])?(?:\.[A-Za-z_][A-Za-z0-9_]*(?:\[[0-9]+\])?)*\z",
        RegexOptions.CultureInvariant);

    // The largest amount a Stellar int64 of stroops can represent.
    private const decimal MaxAmount = 922337203685.4775807m;

    private const int MaxMemoTextBytes = 28;
    private const int MaxMemoHashBytes = 32;
    private const int SignatureBytes = 64;

    /// <summary>Parses and validates a SEP-7 URI.</summary>
    /// <param name="uri">The URI.</param>
    /// <param name="chainDepth">How deep in a <c>chain</c> this URI is nested; 0 for the outermost request.</param>
    /// <param name="requireSignedOriginDomain">
    ///     <c>true</c> to reject an <c>origin_domain</c> without a <c>signature</c>, which SEP-7 declares an invalid
    ///     request. Only the requester's own not-yet-signed URI (while generating and signing it) passes <c>false</c>;
    ///     it applies to that URI alone, and a <c>chain</c> nested in it, being a request someone else sent, is always
    ///     held to the rule.
    /// </param>
    internal static Sep7Uri Parse(string uri, int chainDepth, bool requireSignedOriginDomain)
    {
        if (!uri.StartsWith(UriScheme.SchemePrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid($"The URI must start with '{UriScheme.SchemePrefix}'.");
        }
        if (uri.IndexOf('#') >= 0)
        {
            throw Invalid("The URI must not contain a fragment ('#'); parameter values must be URL-encoded.");
        }
        // A signature covers the URI's UTF-8 bytes, and UTF-8 encoding turns every lone surrogate into U+FFFD, so
        // URIs differing only in one would share a signature. Uri.UnescapeDataString keeps a raw one in the decoded
        // value (an escaped one, such as %ED%A0%80, stays escaped), so this check on the raw URI covers every value.
        if (HasLoneSurrogate(uri))
        {
            throw Invalid("The URI must not contain a lone UTF-16 surrogate, which has no UTF-8 encoding.");
        }

        var rest = uri.Substring(UriScheme.SchemePrefix.Length);
        var queryStart = rest.IndexOf('?');
        var operationName = queryStart < 0 ? rest : rest.Substring(0, queryStart);
        var query = queryStart < 0 ? string.Empty : rest.Substring(queryStart + 1);

        Sep7OperationType operation;
        switch (operationName)
        {
            case UriScheme.OperationTx:
                operation = Sep7OperationType.Tx;
                break;
            case UriScheme.OperationPay:
                operation = Sep7OperationType.Pay;
                break;
            default:
                throw Invalid($"Operation type {Echo(operationName)} is not supported; expected 'tx' or 'pay'.");
        }

        var parameters = ParseQuery(query);

        foreach (var name in parameters.Keys)
        {
            if (operation == Sep7OperationType.Tx && PayOnlyParameters.Contains(name))
            {
                throw Invalid($"Parameter {Echo(name)} is not allowed for the 'tx' operation.");
            }
            if (operation == Sep7OperationType.Pay && TxOnlyParameters.Contains(name))
            {
                throw Invalid($"Parameter {Echo(name)} is not allowed for the 'pay' operation.");
            }
            if (KnownParameters.Contains(name) && parameters[name].Length == 0)
            {
                throw Invalid($"Parameter {Echo(name)} must not be empty.");
            }
        }

        IReadOnlyList<Sep7Replacement> replacements = Array.Empty<Sep7Replacement>();
        Sep7Uri? chainedUri = null;

        if (operation == Sep7OperationType.Tx)
        {
            ValidateXdr(Get(parameters, Sep7Parameters.Xdr));

            var pubkey = Get(parameters, Sep7Parameters.Pubkey);
            if (pubkey != null && !IsCanonical(pubkey, StrKey.IsValidEd25519PublicKey))
            {
                throw Invalid("The 'pubkey' parameter must be a valid account id (G...).");
            }

            var replace = Get(parameters, Sep7Parameters.Replace);
            if (replace != null)
            {
                replacements = ParseReplacements(replace);
            }

            var chain = Get(parameters, Sep7Parameters.Chain);
            if (chain != null)
            {
                if (chainDepth >= UriScheme.MaxChainNestingLevels)
                {
                    throw Invalid(
                        $"The 'chain' parameter nests more than {UriScheme.MaxChainNestingLevels} levels.");
                }
                try
                {
                    chainedUri = Parse(chain, chainDepth + 1, true);
                }
                catch (InvalidOriginDomainException)
                {
                    // Keep the documented subtype; its OriginDomain property names the offending value.
                    throw;
                }
                catch (InvalidSep7UriException ex)
                {
                    throw new InvalidSep7UriException(
                        $"The 'chain' parameter is not a valid SEP-7 URI: {ex.Message}", ex);
                }
            }
        }
        else
        {
            ValidatePayParameters(parameters);
        }

        ValidateCommonParameters(parameters, requireSignedOriginDomain);

        return new Sep7Uri(uri, operation, new ReadOnlyDictionary<string, string>(parameters), replacements,
            chainedUri);
    }

    /// <summary>
    ///     Parses a <c>replace</c> value:
    ///     <c>path_1:id_1,path_2:id_2;id_1:hint_1,id_2:hint_2</c>. Rejects entries without a path or identifier,
    ///     duplicate paths or hint identifiers, and identifiers that are not balanced across the <c>;</c> (the spec
    ///     requires the same set on both sides). A hint may contain <c>:</c>; only the first one separates it from
    ///     its identifier.
    /// </summary>
    internal static IReadOnlyList<Sep7Replacement> ParseReplacements(string value)
    {
        if (value.Length == 0)
        {
            return Array.Empty<Sep7Replacement>();
        }

        var hintsStart = value.IndexOf(UriScheme.ReplaceHintDelimiter);
        if (hintsStart < 0)
        {
            throw Invalid("The 'replace' parameter has no hints section; expected 'fields;hints'.");
        }
        if (value.IndexOf(UriScheme.ReplaceHintDelimiter, hintsStart + 1) >= 0)
        {
            throw Invalid("The 'replace' parameter has more than one ';'.");
        }

        var fields = new List<KeyValuePair<string, string>>();
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in value.Substring(0, hintsStart).Split(UriScheme.ReplaceListDelimiter))
        {
            var parts = item.Split(UriScheme.ReplaceIdDelimiter);
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
            {
                throw Invalid(
                    $"The 'replace' entry {Echo(item)} must have the form 'txrep_path:reference_identifier'.");
            }
            var pathError = ValidateTxrepPath(parts[0]);
            if (pathError != null)
            {
                throw Invalid(pathError);
            }
            if (!paths.Add(parts[0]))
            {
                throw Invalid($"The 'replace' parameter lists the field {Echo(parts[0])} more than once.");
            }
            fields.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
        }

        var hints = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in value.Substring(hintsStart + 1).Split(UriScheme.ReplaceListDelimiter))
        {
            var separator = item.IndexOf(UriScheme.ReplaceIdDelimiter);
            if (separator <= 0 || separator == item.Length - 1)
            {
                throw Invalid($"The 'replace' hint {Echo(item)} must have the form 'reference_identifier:hint'.");
            }
            var id = item.Substring(0, separator);
            if (hints.ContainsKey(id))
            {
                throw Invalid($"The 'replace' parameter has more than one hint for {Echo(id)}.");
            }
            hints.Add(id, item.Substring(separator + 1));
        }

        var fieldIds = new HashSet<string>(fields.Select(f => f.Value), StringComparer.Ordinal);
        if (!fieldIds.SetEquals(hints.Keys))
        {
            throw Invalid(
                "The 'replace' parameter has unbalanced reference identifiers: every identifier must appear " +
                "on both sides of the ';'.");
        }

        return fields.Select(f => new Sep7Replacement(f.Value, f.Key, hints[f.Value])).ToList().AsReadOnly();
    }

    /// <summary>
    ///     Checks a Txrep path as SEP-7 allows it in <c>replace</c>: a well-formed path, without the <c>tx.</c>
    ///     prefix, not a signature and not a metadata field (<c>_present</c>, <c>len</c>).
    /// </summary>
    /// <returns>The reason the path is invalid, or <c>null</c>.</returns>
    internal static string? ValidateTxrepPath(string path)
    {
        if (!TxrepPathRegex.IsMatch(path))
        {
            return $"The 'replace' path {Echo(path)} is not a valid Txrep field path.";
        }

        var segments = path.Split('.').Select(s => s.Split('[')[0]).ToArray();
        if (segments[0] == "tx")
        {
            return $"The 'replace' path {Echo(path)} must not include the 'tx.' prefix.";
        }
        if (segments.Contains("signatures"))
        {
            return $"The 'replace' path {Echo(path)} points at a signature, which cannot be replaced.";
        }
        var last = segments[segments.Length - 1];
        if (last is "_present" or "len")
        {
            return $"The 'replace' path {Echo(path)} is a Txrep metadata field, which cannot be replaced.";
        }
        return null;
    }

    /// <summary>
    ///     Whether <paramref name="value" /> has, between <paramref name="start" /> and <paramref name="end" />, a
    ///     whitespace, control or invisible formatting code point, or a lone surrogate. Categories are taken per code
    ///     point, not per UTF-16 unit: a surrogate half reports <c>Surrogate</c>, which would let every
    ///     supplementary-plane format character (the tag characters U+E0000–U+E007F among them) through.
    /// </summary>
    private static bool HasHiddenCharacters(string value, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            var c = value[i];
            if (char.IsHighSurrogate(c) && i + 1 < end && char.IsLowSurrogate(value[i + 1]))
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(char.ConvertToUtf32(c, value[i + 1]));
                if (category == UnicodeCategory.Format)
                {
                    return true;
                }
                i++;
                continue;
            }
            if (char.IsSurrogate(c) || char.IsWhiteSpace(c) || char.IsControl(c) ||
                CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///     The checks every callback URL passes, in a parsed request and in
    ///     <see cref="UriScheme.SubmitToCallbackAsync" /> alike: no whitespace, control or invisible formatting
    ///     characters, no credentials, no fragment, no malformed percent escape and no backslash. The last three make
    ///     the URL shown differ from the one POSTed to: HTTP never sends a fragment, and <see cref="System.Uri" />
    ///     re-escapes a stray <c>%</c> (<c>/%ZZ</c> is requested as <c>/%25ZZ</c>) and rewrites a backslash
    ///     (<c>/a\b</c> is requested as <c>/a/b</c>, <c>?q=\</c> as <c>?q=%5C</c>). Returns why <paramref name="url" />
    ///     (without its <c>url:</c> prefix) is refused, or <c>null</c>.
    /// </summary>
    internal static string? CallbackUrlProblem(string url, System.Uri parsed)
    {
        // Uri.TryCreate trims surrounding whitespace, so check the raw value: CallbackUrl returns it as sent.
        if (HasHiddenCharacters(url, 0, url.Length))
        {
            return "must not contain whitespace, control or invisible formatting characters";
        }
        if (HasUserInfo(parsed))
        {
            return "must not contain credentials (user info)";
        }
        // KeepDelimiter so an empty trailing '#' counts too.
        if (parsed.GetComponents(UriComponents.Fragment | UriComponents.KeepDelimiter, UriFormat.UriEscaped).Length > 0)
        {
            return "must not contain a fragment ('#'), which HTTP never sends";
        }
        if (HasMalformedPercentEscape(url))
        {
            return "must not contain a '%' that is not followed by two hex digits";
        }
        if (url.IndexOf('\\') >= 0)
        {
            return "must not contain a backslash, which is not sent as written";
        }
        return null;
    }

    private static bool HasMalformedPercentEscape(string value)
    {
        for (var i = value.IndexOf('%'); i >= 0; i = value.IndexOf('%', i + 1))
        {
            if (i + 2 >= value.Length || !IsHexDigit(value[i + 1]) || !IsHexDigit(value[i + 2]))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsHexDigit(char c)
    {
        return c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
    }

    /// <summary>
    ///     Whether <paramref name="url" /> carries user info, even an empty one: <see cref="System.Uri.UserInfo" /> is
    ///     "" for <c>https://@host</c>, and <c>https:\\@host</c> normalizes to that, so the delimiter is what counts.
    /// </summary>
    internal static bool HasUserInfo(System.Uri url)
    {
        return url.GetComponents(UriComponents.UserInfo | UriComponents.KeepDelimiter, UriFormat.UriEscaped).Length > 0;
    }

    internal static bool IsFullyQualifiedDomainName(string domain)
    {
        // RFC 6761 reserves the localhost TLD for loopback: foo.localhost resolves to 127.0.0.1 without any DNS the
        // name's author controls, so it is no more a public domain than "localhost" itself.
        return FqdnRegex.IsMatch(domain) && !domain.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsStrictBase64(string value)
    {
        return value.Length % 4 == 0 && Base64Regex.IsMatch(value);
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        if (query.Length == 0)
        {
            return parameters;
        }

        var rawValues = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var segment in query.Split('&'))
        {
            var separator = segment.IndexOf('=');
            if (separator <= 0)
            {
                throw Invalid($"The query segment {Echo(segment)} is not a 'name=value' pair.");
            }

            var name = DecodeComponent(segment.Substring(0, separator), true);
            if (rawValues.ContainsKey(name))
            {
                throw Invalid($"Parameter {Echo(name)} appears more than once.");
            }
            rawValues.Add(name, segment.Substring(separator + 1));
        }

        // Values are decoded as application/x-www-form-urlencoded, like browsers' URLSearchParams and the other
        // Stellar SDKs: '+' is a space, so "msg=pay+me" from the typescript-wallet-sdk reads "pay me" and its
        // testnet network_passphrase matches. Base64 values are the exception: a space is never valid base64, so a
        // raw '+' there can only be a producer that forgot to percent-encode it, and it is kept as '+'.
        var memoType = rawValues.TryGetValue(Sep7Parameters.MemoType, out var rawMemoType)
            ? DecodeComponent(rawMemoType, true)
            : null;
        foreach (var pair in rawValues)
        {
            parameters.Add(pair.Key, DecodeComponent(pair.Value, !IsBase64Valued(pair.Key, memoType)));
        }
        return parameters;
    }

    private static string DecodeComponent(string component, bool plusIsSpace)
    {
        return Uri.UnescapeDataString(plusIsSpace ? component.Replace('+', ' ') : component);
    }

    private static bool IsBase64Valued(string name, string? memoType)
    {
        return name == Sep7Parameters.Xdr || name == Sep7Parameters.Signature ||
               (name == Sep7Parameters.Memo && memoType is Sep7MemoType.MemoHash or Sep7MemoType.MemoReturn);
    }

    private static void ValidateXdr(string? xdr)
    {
        if (xdr == null)
        {
            throw Invalid("Missing required parameter 'xdr' for the 'tx' operation.");
        }
        try
        {
            DecodeEnvelope(xdr);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new InvalidSep7UriException(
                "The 'xdr' parameter is not a valid base64 XDR transaction envelope.", ex);
        }
    }

    /// <summary>
    ///     Decodes exactly one <c>TransactionEnvelope</c>. The value must use only the base64 alphabet and padding
    ///     (whitespace, which <see cref="Convert.FromBase64String" /> skips, is rejected) and trailing bytes are
    ///     rejected, so the transaction a wallet signs is the whole of what the request carried.
    /// </summary>
    internal static TransactionBase DecodeEnvelope(string xdr)
    {
        if (!IsStrictBase64(xdr))
        {
            throw new FormatException("The value is not strict base64.");
        }
        var stream = new Xdr.XdrDataInputStream(Convert.FromBase64String(xdr));
        var envelope = Xdr.TransactionEnvelope.Decode(stream);
        if (stream.GetRemainingInputLen() != 0)
        {
            throw new FormatException(
                $"{stream.GetRemainingInputLen()} unexpected trailing byte(s) after the transaction envelope.");
        }
        return TransactionBuilder.FromEnvelopeXdr(envelope);
    }

    private static void ValidatePayParameters(Dictionary<string, string> parameters)
    {
        var destination = Get(parameters, Sep7Parameters.Destination);
        if (destination == null)
        {
            throw Invalid("Missing required parameter 'destination' for the 'pay' operation.");
        }
        if (!IsCanonical(destination, StrKey.IsValidEd25519PublicKey) &&
            !IsCanonical(destination, StrKey.IsValidMed25519PublicKey) &&
            !IsCanonical(destination, StrKey.IsValidContractId) &&
            !IsFederationAddress(destination))
        {
            throw Invalid(
                "The 'destination' parameter must be a valid account id (G...), muxed account (M...), contract id " +
                "(C...) or federation address (name*domain).");
        }

        var amount = Get(parameters, Sep7Parameters.Amount);
        if (amount != null && !IsValidAmount(amount))
        {
            throw Invalid(
                $"The 'amount' parameter {Echo(amount)} must be a positive decimal number with at most 7 decimal " +
                "places.");
        }

        var assetCode = Get(parameters, Sep7Parameters.AssetCode);
        var assetIssuer = Get(parameters, Sep7Parameters.AssetIssuer);
        if ((assetCode == null) != (assetIssuer == null))
        {
            throw Invalid(
                "The 'asset_code' and 'asset_issuer' parameters must be given together (both absent means XLM).");
        }
        if (assetCode != null && !AssetCodeRegex.IsMatch(assetCode))
        {
            throw Invalid("The 'asset_code' parameter must be 1 to 12 letters or digits.");
        }
        if (assetIssuer != null && !IsCanonical(assetIssuer, StrKey.IsValidEd25519PublicKey))
        {
            throw Invalid("The 'asset_issuer' parameter must be a valid account id (G...).");
        }

        var memo = Get(parameters, Sep7Parameters.Memo);
        var memoType = Get(parameters, Sep7Parameters.MemoType);
        if (memoType != null)
        {
            if (memoType is not (Sep7MemoType.MemoText or Sep7MemoType.MemoId or Sep7MemoType.MemoHash
                or Sep7MemoType.MemoReturn))
            {
                throw Invalid(
                    $"The 'memo_type' parameter {Echo(memoType)} is not one of MEMO_TEXT, MEMO_ID, MEMO_HASH, " +
                    "MEMO_RETURN.");
            }
            if (memo == null)
            {
                throw Invalid("The 'memo_type' parameter requires a 'memo' parameter.");
            }
        }
        if (memo != null)
        {
            ValidateMemo(memo, memoType ?? Sep7MemoType.MemoText);
        }
    }

    private static void ValidateMemo(string memo, string memoType)
    {
        switch (memoType)
        {
            case Sep7MemoType.MemoId:
                if (!ulong.TryParse(memo, NumberStyles.None, CultureInfo.InvariantCulture, out _))
                {
                    throw Invalid("A MEMO_ID 'memo' must be an unsigned 64-bit integer.");
                }
                break;
            case Sep7MemoType.MemoHash:
            case Sep7MemoType.MemoReturn:
                if (!IsStrictBase64(memo))
                {
                    throw Invalid($"A {memoType} 'memo' must be base64-encoded.");
                }
                // Exactly 32 bytes: a shorter value would be zero-padded into a different hash than the one sent.
                if (Convert.FromBase64String(memo).Length != MaxMemoHashBytes)
                {
                    throw Invalid($"A {memoType} 'memo' must decode to exactly {MaxMemoHashBytes} bytes.");
                }
                break;
            default:
                if (Encoding.UTF8.GetByteCount(memo) > MaxMemoTextBytes)
                {
                    throw Invalid($"A MEMO_TEXT 'memo' must be at most {MaxMemoTextBytes} bytes of UTF-8.");
                }
                break;
        }
    }

    private static void ValidateCommonParameters(Dictionary<string, string> parameters, bool requireSignedOriginDomain)
    {
        var callback = Get(parameters, Sep7Parameters.Callback);
        if (callback != null)
        {
            if (!callback.StartsWith(UriScheme.CallbackUrlPrefix, StringComparison.Ordinal))
            {
                throw Invalid("The 'callback' parameter must start with 'url:' (the only callback type SEP-7 defines).");
            }
            var url = callback.Substring(UriScheme.CallbackUrlPrefix.Length);
            if (!System.Uri.TryCreate(url, UriKind.Absolute, out var callbackUri) ||
                (callbackUri.Scheme != System.Uri.UriSchemeHttps && callbackUri.Scheme != System.Uri.UriSchemeHttp))
            {
                throw Invalid("The 'callback' parameter must hold an absolute http(s) URL after 'url:'.");
            }
            if (CallbackUrlProblem(url, callbackUri) is { } problem)
            {
                throw Invalid($"The 'callback' URL {problem}.");
            }
        }

        var message = Get(parameters, Sep7Parameters.Msg);
        if (message != null && CountCodePoints(message) > UriScheme.MaxMessageLength)
        {
            throw Invalid($"The 'msg' parameter must be at most {UriScheme.MaxMessageLength} characters.");
        }

        var originDomain = Get(parameters, Sep7Parameters.OriginDomain);
        if (originDomain != null && !IsFullyQualifiedDomainName(originDomain))
        {
            throw new InvalidOriginDomainException(originDomain);
        }

        var signature = Get(parameters, Sep7Parameters.Signature);
        if (signature != null &&
            (!IsStrictBase64(signature) || Convert.FromBase64String(signature).Length != SignatureBytes))
        {
            throw Invalid($"The 'signature' parameter must be a base64-encoded {SignatureBytes}-byte signature.");
        }

        // SEP-7: "If there is an origin_domain but no signature, the URI request is invalid", and the wallet must
        // not let the user sign it.
        if (requireSignedOriginDomain && originDomain != null && signature == null)
        {
            throw Invalid(
                "The URI has an 'origin_domain' but no 'signature' parameter; SEP-7 treats such a request as invalid.");
        }
    }

    /// <summary>
    ///     A valid strkey in its canonical upper-case form. The strkey decoder also accepts lower case, but a
    ///     lower-case key then compares unequal to the same key everywhere else (the signer's
    ///     <c>AccountId</c>, a pinned key), so only the canonical spelling is accepted.
    /// </summary>
    private static bool IsCanonical(string value, Func<string, bool> isValid)
    {
        return isValid(value) && value == value.ToUpperInvariant();
    }

    /// <summary>
    ///     A SEP-2 federation address, <c>name*domain</c>: SEP-7 allows one as a <c>pay</c> destination (the
    ///     wallet resolves it). As SEP-2 requires, the name is non-empty and has no whitespace, <c>*</c> or
    ///     <c>&gt;</c>; control and invisible formatting characters are rejected too. The domain is a fully
    ///     qualified domain name.
    /// </summary>
    internal static bool IsFederationAddress(string value)
    {
        var separator = value.IndexOf('*');
        if (separator <= 0)
        {
            return false;
        }
        if (value.IndexOf('>', 0, separator) >= 0 || HasHiddenCharacters(value, 0, separator))
        {
            return false;
        }
        return IsFullyQualifiedDomainName(value.Substring(separator + 1));
    }

    private static bool IsValidAmount(string amount)
    {
        return AmountRegex.IsMatch(amount) &&
               decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                   out var value) &&
               value > 0 && value <= MaxAmount;
    }

    private static bool HasLoneSurrogate(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                i++;
            }
            else if (char.IsSurrogate(value[i]))
            {
                return true;
            }
        }
        return false;
    }

    private static int CountCodePoints(string value)
    {
        var count = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
            {
                i++;
            }
            count++;
        }
        return count;
    }

    private static string? Get(Dictionary<string, string> parameters, string name)
    {
        return parameters.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    ///     Renders a value taken from the URI for an exception message: quoted, escaped and length-clamped, since
    ///     the URI is untrusted and messages end up in logs and in <see cref="Sep7ValidationResult.Reason" />.
    /// </summary>
    internal static string Echo(string value)
    {
        return UntrustedJsonValue.Describe(value);
    }

    private static InvalidSep7UriException Invalid(string reason)
    {
        return new InvalidSep7UriException(reason);
    }
}
