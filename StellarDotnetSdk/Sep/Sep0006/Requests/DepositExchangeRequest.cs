using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for initiating a deposit with asset conversion.
///     A deposit exchange allows a user to send an off-chain asset to an anchor
///     and receive a different Stellar asset in return. This leverages SEP-38 quotes.
/// </summary>
public sealed record DepositExchangeRequest
{
    /// <summary>
    ///     The code of the on-chain asset the user wants to get from the Anchor
    ///     after doing an off-chain deposit. The value passed must match one of the
    ///     codes listed in the /info response's deposit-exchange object.
    /// </summary>
    public required string DestinationAsset { get; init; }

    /// <summary>
    ///     The off-chain asset the Anchor will receive from the user. The value must
    ///     match one of the asset values included in a SEP-38 GET /prices response
    ///     using SEP-38 Asset Identification Format.
    /// </summary>
    public required string SourceAsset { get; init; }

    /// <summary>
    ///     The amount of the source_asset the user would like to deposit to the
    ///     anchor's off-chain account.
    /// </summary>
    public required string Amount { get; init; }

    /// <summary>
    ///     The stellar or muxed account ID of the user that wants to deposit.
    ///     This is where the asset token will be sent.
    /// </summary>
    public required string Account { get; init; }

    /// <summary>
    ///     The id returned from a SEP-38 POST /quote response.
    /// </summary>
    public string? QuoteId { get; init; }

    /// <summary>
    ///     Type of memo that the anchor should attach to the Stellar
    ///     payment transaction, one of text, id or hash.
    /// </summary>
    public string? MemoType { get; init; }

    /// <summary>
    ///     Value of memo to attach to transaction, for hash this should
    ///     be base64-encoded.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    ///     Email address of depositor.
    /// </summary>
    public string? EmailAddress { get; init; }

    /// <summary>
    ///     Type of deposit. If the anchor supports multiple deposit
    ///     methods (e.g. SEPA or SWIFT), the wallet should specify type.
    /// </summary>
    public string? Type { get; init; }

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
    ///     Id of an off-chain account (managed by the anchor) associated
    ///     with this user's Stellar account.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    ///     Id of the chosen location to drop off cash.
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

