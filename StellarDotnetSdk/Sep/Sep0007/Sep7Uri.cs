using System;
using System.Collections.Generic;
using System.Globalization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>
///     A parsed and validated SEP-7 request (<c>web+stellar:tx?...</c> or <c>web+stellar:pay?...</c>).
///     Instances come from <see cref="UriScheme.ParseUri" /> or <see cref="UriScheme.TryParseUri" />, so every
///     parameter has already passed the checks described there. Parameter values are URL-decoded.
/// </summary>
public sealed class Sep7Uri
{
    internal Sep7Uri(
        string uri,
        Sep7OperationType operationType,
        IReadOnlyDictionary<string, string> parameters,
        IReadOnlyList<Sep7Replacement> replacements,
        Sep7Uri? chainedUri)
    {
        Uri = uri;
        OperationType = operationType;
        Parameters = parameters;
        Replacements = replacements;
        ChainedUri = chainedUri;
    }

    /// <summary>
    ///     The URI exactly as it was parsed. Request signatures cover this literal string (minus the
    ///     <c>signature</c> parameter), so it is kept as-is rather than re-serialized.
    /// </summary>
    public string Uri { get; }

    /// <summary>The requested operation.</summary>
    public Sep7OperationType OperationType { get; }

    /// <summary>
    ///     Every query parameter of the URI, URL-decoded, keyed by parameter name. Includes parameters this SDK
    ///     does not know about, which SEP-7 leaves room for as future extensions.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>The base64 XDR transaction envelope (<c>xdr</c>); <c>tx</c> requests only.</summary>
    public string? Xdr => Get(Sep7Parameters.Xdr);

    /// <summary>The raw <c>replace</c> value; <c>tx</c> requests only. See <see cref="Replacements" />.</summary>
    public string? Replace => Get(Sep7Parameters.Replace);

    /// <summary>
    ///     The fields of the transaction the requester wants the wallet to fill in, parsed from <c>replace</c>;
    ///     empty when there is none.
    /// </summary>
    public IReadOnlyList<Sep7Replacement> Replacements { get; }

    /// <summary>The raw <c>callback</c> value, including its <c>url:</c> prefix.</summary>
    public string? Callback => Get(Sep7Parameters.Callback);

    /// <summary>The URL to POST the signed transaction to: <see cref="Callback" /> without its <c>url:</c> prefix.</summary>
    public string? CallbackUrl => Callback?.Substring(UriScheme.CallbackUrlPrefix.Length);

    /// <summary>The public key the requester wants the transaction signed with (<c>pubkey</c>); <c>tx</c> requests only.</summary>
    public string? PublicKey => Get(Sep7Parameters.Pubkey);

    /// <summary>The raw embedded SEP-7 request (<c>chain</c>); <c>tx</c> requests only.</summary>
    public string? Chain => Get(Sep7Parameters.Chain);

    /// <summary>
    ///     The embedded SEP-7 request, parsed; <c>null</c> when there is no <c>chain</c> parameter. It is parsed,
    ///     not verified: its <c>origin_domain</c> signature is not checked, so do not show its
    ///     <see cref="OriginDomain" /> as trusted without running
    ///     <see cref="UriScheme.VerifyOriginDomainSignatureAsync" /> on <see cref="Chain" />.
    /// </summary>
    public Sep7Uri? ChainedUri { get; }

    /// <summary>The message for the user (<c>msg</c>).</summary>
    public string? Message => Get(Sep7Parameters.Msg);

    /// <summary>
    ///     The network passphrase (<c>network_passphrase</c>); <c>null</c> means the public network. See
    ///     <see cref="GetNetwork" />.
    /// </summary>
    public string? NetworkPassphrase => Get(Sep7Parameters.NetworkPassphrase);

    /// <summary>
    ///     The domain the request claims to come from (<c>origin_domain</c>). Do not show it to the user before
    ///     <see cref="UriScheme.VerifyOriginDomainSignatureAsync" /> has verified the request signature.
    /// </summary>
    public string? OriginDomain => Get(Sep7Parameters.OriginDomain);

    /// <summary>The base64 request signature (<c>signature</c>), URL-decoded.</summary>
    public string? Signature => Get(Sep7Parameters.Signature);

    /// <summary>
    ///     The account id, muxed account, contract id or federation address (<c>name*domain</c>, which the wallet
    ///     resolves) to pay (<c>destination</c>); <c>pay</c> requests only.
    /// </summary>
    public string? Destination => Get(Sep7Parameters.Destination);

    /// <summary>The amount to pay (<c>amount</c>); <c>pay</c> requests only.</summary>
    public string? Amount => Get(Sep7Parameters.Amount);

    /// <summary>The code of the asset to pay (<c>asset_code</c>); <c>pay</c> requests only.</summary>
    public string? AssetCode => Get(Sep7Parameters.AssetCode);

    /// <summary>The issuer of the asset to pay (<c>asset_issuer</c>); <c>pay</c> requests only.</summary>
    public string? AssetIssuer => Get(Sep7Parameters.AssetIssuer);

    /// <summary>
    ///     The memo value (<c>memo</c>) as sent: text, a decimal id, or base64 for hash memos; <c>pay</c> requests
    ///     only. See <see cref="GetMemo" />.
    /// </summary>
    public string? Memo => Get(Sep7Parameters.Memo);

    /// <summary>The memo type (<c>memo_type</c>), one of the <see cref="Sep7MemoType" /> values; <c>pay</c> requests only.</summary>
    public string? MemoType => Get(Sep7Parameters.MemoType);

    /// <summary>Decodes the transaction envelope of a <c>tx</c> request. Each call returns a new instance.</summary>
    /// <returns>The transaction to sign.</returns>
    /// <exception cref="InvalidOperationException">Thrown for a <c>pay</c> request.</exception>
    public TransactionBase GetTransaction()
    {
        if (OperationType != Sep7OperationType.Tx || Xdr is null)
        {
            throw new InvalidOperationException("Only 'tx' requests carry a transaction envelope.");
        }
        return Sep7UriParser.DecodeEnvelope(Xdr);
    }

    /// <summary>
    ///     The network the request is for: <see cref="NetworkPassphrase" /> when present, otherwise the public
    ///     network, as SEP-7 specifies.
    /// </summary>
    /// <returns>The network to sign for.</returns>
    public Network GetNetwork()
    {
        return new Network(NetworkPassphrase ?? Network.PublicPassphrase);
    }

    /// <summary>
    ///     Decodes the memo of a <c>pay</c> request. A <c>memo</c> without <c>memo_type</c> is read as
    ///     <c>MEMO_TEXT</c>.
    /// </summary>
    /// <returns>The memo, or <c>null</c> when the request has none.</returns>
    public Memo? GetMemo()
    {
        if (Memo is null)
        {
            return null;
        }

        return (MemoType ?? Sep7MemoType.MemoText) switch
        {
            Sep7MemoType.MemoId => Memos.Memo.Id(ulong.Parse(Memo, NumberStyles.None, CultureInfo.InvariantCulture)),
            Sep7MemoType.MemoHash => Memos.Memo.Hash(Convert.FromBase64String(Memo)),
            Sep7MemoType.MemoReturn => Memos.Memo.ReturnHash(Convert.FromBase64String(Memo)),
            _ => Memos.Memo.Text(Memo),
        };
    }

    /// <summary>
    ///     The asset a <c>pay</c> request asks for: native XLM when <c>asset_code</c> and <c>asset_issuer</c> are
    ///     absent, as SEP-7 specifies.
    /// </summary>
    /// <returns>The asset to pay with.</returns>
    /// <exception cref="InvalidOperationException">Thrown for a <c>tx</c> request.</exception>
    public Asset GetAsset()
    {
        if (OperationType != Sep7OperationType.Pay)
        {
            throw new InvalidOperationException("Only 'pay' requests name an asset.");
        }
        return AssetCode is null || AssetIssuer is null
            ? new AssetTypeNative()
            : Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Uri;
    }

    private string? Get(string name)
    {
        return Parameters.TryGetValue(name, out var value) ? value : null;
    }
}
