using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for initiating a withdrawal transaction.
///     A withdrawal occurs when a user sends a Stellar asset to an anchor's account,
///     and the anchor delivers the equivalent amount in an off-chain asset.
/// </summary>
public sealed record WithdrawRequest
{
    /// <summary>
    ///     Code of the on-chain asset the user wants to withdraw.
    ///     The value passed must match one of the codes listed in the /info response's withdraw object.
    /// </summary>
    public required string AssetCode { get; init; }

    /// <summary>
    ///     Type of withdrawal. Can be: crypto, bank_account, cash, mobile,
    ///     bill_payment or other custom values.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    ///     (Deprecated) The account that the user wants to withdraw their funds to.
    ///     This can be a crypto account, a bank account number, IBAN, mobile number,
    ///     or email address.
    /// </summary>
    public string? Dest { get; init; }

    /// <summary>
    ///     (Deprecated, optional) Extra information to specify withdrawal location.
    ///     For crypto it may be a memo in addition to the dest address.
    /// </summary>
    public string? DestExtra { get; init; }

    /// <summary>
    ///     The Stellar or muxed account the client will use as the source
    ///     of the withdrawal payment to the anchor.
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    ///     This field should only be used if SEP-10 authentication is not.
    ///     It was originally intended to distinguish users of the same Stellar account.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    ///     (Deprecated, optional) Type of memo. One of text, id or hash.
    /// </summary>
    public string? MemoType { get; init; }

    /// <summary>
    ///     (Deprecated) Wallet name for display.
    /// </summary>
    public string? WalletName { get; init; }

    /// <summary>
    ///     (Deprecated) Wallet URL for notifications.
    /// </summary>
    public string? WalletUrl { get; init; }

    /// <summary>
    ///     Defaults to en if not specified. Language code specified using RFC 4646.
    /// </summary>
    public string? Lang { get; init; }

    /// <summary>
    ///     A URL that the anchor should POST a JSON message to when the
    ///     status property of the transaction created as a result of this request
    ///     changes.
    /// </summary>
    public string? OnChangeCallback { get; init; }

    /// <summary>
    ///     The amount of the asset the user would like to withdraw.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    ///     The ISO 3166-1 alpha-3 code of the user's current address.
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    ///     The memo the anchor must use when sending refund payments back
    ///     to the user.
    /// </summary>
    public string? RefundMemo { get; init; }

    /// <summary>
    ///     The type of the refund_memo. Can be id, text, or hash.
    /// </summary>
    public string? RefundMemoType { get; init; }

    /// <summary>
    ///     Id of an off-chain account (managed by the anchor) associated
    ///     with this user's Stellar account.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    ///     Id of the chosen location to pick up cash.
    /// </summary>
    public string? LocationId { get; init; }

    /// <summary>
    ///     Can be used to provide extra fields for the request.
    /// </summary>
    public Dictionary<string, string>? ExtraFields { get; init; }

    /// <summary>
    ///     JWT previously received from the anchor via the SEP-10 authentication flow.
    /// </summary>
    public string? Jwt { get; init; }
}

