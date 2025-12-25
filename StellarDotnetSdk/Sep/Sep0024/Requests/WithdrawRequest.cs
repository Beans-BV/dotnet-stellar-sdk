using System;
using System.Collections.Generic;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Sep.Sep0024.Requests;

/// <summary>
///     Request to initiate an interactive withdrawal flow with an anchor.
///     A withdrawal allows a user to redeem a Stellar asset for the real-world asset
///     (fiat via bank transfer, BTC, USD cash, etc.) via the anchor. The user sends
///     the Stellar asset to the anchor, and the anchor sends the equivalent off-chain
///     asset (minus fees) to the user.
///     The withdrawal endpoint returns an interactive URL where the user completes KYC,
///     provides bank account or wallet details, and receives instructions for the withdrawal.
///     Authentication: Always required. Must provide a SEP-10 JWT token.
/// </summary>
public sealed record WithdrawRequest
{
    /// <summary>
    ///     Gets or sets the JWT token previously received from the anchor via the SEP-10 authentication flow.
    ///     Required for authentication.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Gets or sets the code of the Stellar asset the user wants to withdraw.
    ///     Must match one of the codes listed in the /info response's withdraw object.
    ///     Use 'native' to represent the native XLM token.
    /// </summary>
    public required string AssetCode { get; init; }

    /// <summary>
    ///     Gets or sets the issuer of the Stellar asset the user wants to withdraw.
    ///     If not provided, the anchor will use the asset they issue (as described in their TOML file).
    ///     Must not be set if assetCode is 'native'.
    /// </summary>
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     Gets or sets the off-chain asset user wants to receive (Asset Identification Format).
    ///     This is the destination asset (e.g., fiat asset, BTC).
    ///     If not provided, it will be collected in the interactive flow.
    ///     When quoteId is specified, this must match the quote's buy_asset or be omitted.
    /// </summary>
    public string? DestinationAsset { get; init; }

    /// <summary>
    ///     Gets or sets the amount of asset requested to withdraw.
    ///     If not provided, it will be collected in the interactive flow.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    ///     Gets or sets the id returned from a SEP-38 POST /quote response.
    ///     When provided, the withdrawal uses the firm quote for the asset exchange.
    /// </summary>
    public string? QuoteId { get; init; }

    /// <summary>
    ///     Gets or sets the Stellar (G...) or muxed account (M...) that will send the withdrawal payment.
    ///     Defaults to the account authenticated via SEP-10 if not specified.
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    ///     Gets or sets the memo value. This field was originally intended to differentiate users of the same Stellar account.
    ///     However, the anchor should use the sub value included in the decoded SEP-10 JWT instead.
    ///     Anchors should still support this parameter to maintain support for outdated clients.
    ///     Deprecated: Use the sub value in the SEP-10 JWT instead.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    ///     Gets or sets the type of memo. One of: text, id, or hash.
    ///     Deprecated: Memos used to identify users of the same Stellar account should always be of type id.
    /// </summary>
    public string? MemoType { get; init; }

    /// <summary>
    ///     Gets or sets the wallet name that the anchor should display to explain where funds are coming from.
    ///     Used in communications and pages about the withdrawal.
    /// </summary>
    public string? WalletName { get; init; }

    /// <summary>
    ///     Gets or sets the URL the anchor can show when referencing the wallet involved in the withdrawal.
    ///     For example, displayed in the anchor's transaction history.
    /// </summary>
    public string? WalletUrl { get; init; }

    /// <summary>
    ///     Gets or sets the language code specified using RFC 4646 (e.g., en-US).
    ///     Defaults to 'en' if not specified or if the specified language is not supported.
    ///     Error fields, interactive flow UI, and user-facing strings will be in this language.
    /// </summary>
    public string? Lang { get; init; }

    /// <summary>
    ///     Gets or sets the memo the anchor must use when sending refund payments back to the user.
    ///     If not specified, the anchor should use the same memo from the original payment.
    ///     If specified, refundMemoType must also be specified.
    /// </summary>
    public string? RefundMemo { get; init; }

    /// <summary>
    ///     Gets or sets the type of the refundMemo. One of: id, text, or hash.
    ///     If specified, refundMemo must also be specified.
    /// </summary>
    public string? RefundMemoType { get; init; }

    /// <summary>
    ///     Gets or sets the id of an off-chain account (managed by the anchor) associated with this user's Stellar account
    ///     (identified by the JWT's sub field). If the anchor supports SEP-12, the customer_id field should match the SEP-12 customer's id.
    ///     customer_id should be passed only when the off-chain id is known to the client, but the relationship between this id
    ///     and the user's Stellar account is not known to the Anchor.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    ///     Gets or sets the SEP-9 KYC fields to make the onboarding experience simpler.
    ///     These fields may be used to pre-fill the interactive form.
    /// </summary>
    public StandardKycFields? KycFields { get; init; }

    /// <summary>
    ///     Gets or sets custom SEP-9 fields for transmission (fieldname, value).
    /// </summary>
    public Dictionary<string, string>? CustomFields { get; init; }

    /// <summary>
    ///     Gets or sets custom SEP-9 files for transmission (fieldname, value).
    /// </summary>
    public Dictionary<string, byte[]>? CustomFiles { get; init; }
}

