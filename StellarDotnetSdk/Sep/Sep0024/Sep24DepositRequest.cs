using System.Collections.Generic;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Request to initiate an interactive deposit flow with an anchor.
///     A deposit allows a user to send external assets (fiat via bank transfer, BTC,
///     USD cash, etc.) to an anchor, which then sends an equivalent amount of the
///     Stellar asset (minus fees) to the user's Stellar account.
///     The deposit endpoint returns an interactive URL where the user completes KYC,
///     provides payment details, and receives instructions for sending their off-chain
///     assets to the anchor.
///     Authentication: Always required. Must provide a SEP-10 JWT token.
/// </summary>
public class Sep24DepositRequest
{
    /// <summary>
    ///     Gets or sets the JWT token previously received from the anchor via the SEP-10 authentication flow.
    ///     Required for authentication.
    /// </summary>
    public string Jwt { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the code of the Stellar asset the user wants to receive for their deposit.
    ///     Must match one of the codes listed in the /info response's deposit object.
    ///     Use 'native' to represent the native XLM token.
    /// </summary>
    public string AssetCode { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the issuer of the Stellar asset the user wants to receive for their deposit.
    ///     If not provided, the anchor will use the asset they issue (as described in their TOML file).
    ///     Must not be set if assetCode is 'native'.
    /// </summary>
    public string? AssetIssuer { get; set; }

    /// <summary>
    ///     Gets or sets the off-chain asset user wants to send (Asset Identification Format).
    ///     This is the asset the user initially holds (e.g., fiat asset, BTC).
    ///     If not provided, it will be collected in the interactive flow.
    ///     When quoteId is specified, this must match the quote's sell_asset or be omitted.
    /// </summary>
    public string? SourceAsset { get; set; }

    /// <summary>
    ///     Gets or sets the amount of asset requested to deposit.
    ///     If not provided, it will be collected in the interactive flow.
    /// </summary>
    public string? Amount { get; set; }

    /// <summary>
    ///     Gets or sets the id returned from a SEP-38 POST /quote response.
    ///     When provided, the deposit uses the firm quote for the asset exchange.
    /// </summary>
    public string? QuoteId { get; set; }

    /// <summary>
    ///     Gets or sets the Stellar (G...) or muxed account (M...) that will receive the deposit.
    ///     Defaults to the account authenticated via SEP-10 if not specified.
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    ///     Gets or sets the value of memo to attach to the Stellar payment transaction.
    ///     For hash memos, this should be base64-encoded.
    ///     Can be different from the memo in the SEP-10 JWT (e.g., as a reference number).
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    ///     Gets or sets the type of memo that anchor should attach to the Stellar payment transaction.
    ///     One of: text, id, or hash.
    /// </summary>
    public string? MemoType { get; set; }

    /// <summary>
    ///     Gets or sets the wallet name that the anchor should display to explain where funds are going.
    ///     Used in communications and pages about the deposit.
    /// </summary>
    public string? WalletName { get; set; }

    /// <summary>
    ///     Gets or sets the URL the anchor should link to when notifying the user that the transaction has completed.
    /// </summary>
    public string? WalletUrl { get; set; }

    /// <summary>
    ///     Gets or sets the language code specified using RFC 4646 (e.g., en-US).
    ///     Defaults to 'en' if not specified or if the specified language is not supported.
    ///     Error fields, interactive flow UI, and user-facing strings will be in this language.
    /// </summary>
    public string? Lang { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the client supports receiving deposit transactions as a claimable balance.
    ///     This is relevant for users without a trustline to the requested asset.
    /// </summary>
    public string? ClaimableBalanceSupported { get; set; }

    /// <summary>
    ///     Gets or sets the SEP-9 KYC fields to make the onboarding experience simpler.
    ///     These fields may be used to pre-fill the interactive form.
    /// </summary>
    public StandardKycFields? KycFields { get; set; }

    /// <summary>
    ///     Gets or sets custom SEP-9 fields for transmission (fieldname, value).
    /// </summary>
    public Dictionary<string, string>? CustomFields { get; set; }

    /// <summary>
    ///     Gets or sets custom SEP-9 files for transmission (fieldname, value).
    /// </summary>
    public Dictionary<string, byte[]>? CustomFiles { get; set; }
}

