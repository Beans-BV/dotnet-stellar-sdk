using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for initiating a withdrawal with asset conversion.
///     A withdrawal exchange allows a user to send a Stellar asset to an anchor
///     and receive a different off-chain asset in return. This leverages SEP-38 quotes.
/// </summary>
public sealed record WithdrawExchangeRequest
{
    /// <summary>
    ///     Code of the on-chain asset the user wants to withdraw. The value passed
    ///     must match one of the codes listed in the /info response's
    ///     withdraw-exchange object.
    /// </summary>
    public required string SourceAsset { get; init; }

    /// <summary>
    ///     The off-chain asset the Anchor will deliver to the user's account.
    ///     The value must match one of the asset values included in a SEP-38
    ///     GET /prices response using SEP-38 Asset Identification Format.
    /// </summary>
    public required string DestinationAsset { get; init; }

    /// <summary>
    ///     The amount of the on-chain asset (source_asset) the user would like to
    ///     send to the anchor's Stellar account.
    /// </summary>
    public required string Amount { get; init; }

    /// <summary>
    ///     Type of withdrawal. Can be: crypto, bank_account, cash, mobile,
    ///     bill_payment or other custom values.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    ///     (Deprecated) The account that the user wants to withdraw their
    ///     funds to.
    /// </summary>
    public string? Dest { get; init; }

    /// <summary>
    ///     (Deprecated, optional) Extra information to specify withdrawal
    ///     location.
    /// </summary>
    public string? DestExtra { get; init; }

    /// <summary>
    ///     The id returned from a SEP-38 POST /quote response.
    /// </summary>
    public string? QuoteId { get; init; }

    /// <summary>
    ///     The Stellar or muxed account of the user that wants to do the
    ///     withdrawal.
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    ///     This field should only be used if SEP-10 authentication is not.
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
    ///     The ISO 3166-1 alpha-3 code of the user's current address.
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    ///     True if the client supports receiving deposit transactions
    ///     as a claimable balance, false otherwise.
    /// </summary>
    public string? ClaimableBalanceSupported { get; init; }

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

