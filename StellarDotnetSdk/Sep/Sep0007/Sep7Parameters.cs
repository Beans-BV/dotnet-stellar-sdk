namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>The query parameter names defined by SEP-7.</summary>
public static class Sep7Parameters
{
    /// <summary><c>tx</c> only (required): the base64 XDR <c>TransactionEnvelope</c> to sign.</summary>
    public const string Xdr = "xdr";

    /// <summary><c>tx</c> only: the SEP-11 Txrep fields the wallet should fill in, with hints.</summary>
    public const string Replace = "replace";

    /// <summary>Where to send the signed transaction; <c>url:</c> followed by a URL to POST it to.</summary>
    public const string Callback = "callback";

    /// <summary><c>tx</c> only: the public key the requester wants the transaction signed with.</summary>
    public const string Pubkey = "pubkey";

    /// <summary><c>tx</c> only: an embedded SEP-7 request that caused this one (informational).</summary>
    public const string Chain = "chain";

    /// <summary>A message for the user, at most 300 characters before URL-encoding.</summary>
    public const string Msg = "msg";

    /// <summary>The passphrase of the network the request is for; the public network when absent.</summary>
    public const string NetworkPassphrase = "network_passphrase";

    /// <summary>The fully qualified domain name the request comes from; requires <see cref="Signature" />.</summary>
    public const string OriginDomain = "origin_domain";

    /// <summary>The request signature made with the origin domain's <c>URI_REQUEST_SIGNING_KEY</c>.</summary>
    public const string Signature = "signature";

    /// <summary><c>pay</c> only (required): the account id or payment address to pay.</summary>
    public const string Destination = "destination";

    /// <summary><c>pay</c> only: the amount to pay; the wallet asks the user when absent.</summary>
    public const string Amount = "amount";

    /// <summary><c>pay</c> only: the code of the asset to pay; XLM when absent.</summary>
    public const string AssetCode = "asset_code";

    /// <summary><c>pay</c> only: the issuer of the asset to pay; XLM when absent.</summary>
    public const string AssetIssuer = "asset_issuer";

    /// <summary><c>pay</c> only: the memo to attach to the payment.</summary>
    public const string Memo = "memo";

    /// <summary><c>pay</c> only: the type of <see cref="Memo" />; one of the <see cref="Sep7MemoType" /> values.</summary>
    public const string MemoType = "memo_type";
}
