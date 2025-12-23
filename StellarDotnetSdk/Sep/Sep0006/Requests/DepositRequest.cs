using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0006.Requests;

/// <summary>
///     Request parameters for initiating a deposit transaction.
///     A deposit occurs when a user sends an external asset (fiat via bank transfer,
///     crypto from another blockchain, etc.) to an anchor, and the anchor sends an
///     equivalent amount of the corresponding Stellar asset to the user's account.
/// </summary>
public sealed record DepositRequest
{
    /// <summary>
    ///     The code of the on-chain asset the user wants to get from the Anchor
    ///     after doing an off-chain deposit. The value passed must match one of the
    ///     codes listed in the /info response's deposit object.
    /// </summary>
    public required string AssetCode { get; init; }

    /// <summary>
    ///     The stellar or muxed account ID of the user that wants to deposit.
    ///     This is where the asset token will be sent.
    /// </summary>
    public required string Account { get; init; }

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
    ///     Email address of depositor. If desired, an anchor can use
    ///     this to send email updates to the user about the deposit.
    /// </summary>
    public string? EmailAddress { get; init; }

    /// <summary>
    ///     Type of deposit. If the anchor supports multiple deposit
    ///     methods (e.g. SEPA or SWIFT), the wallet should specify type.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    ///     (Deprecated) In communications / pages about the deposit,
    ///     anchor should display the wallet name to the user to explain where funds
    ///     are going.
    /// </summary>
    public string? WalletName { get; init; }

    /// <summary>
    ///     (Deprecated) Anchor should link to this when notifying the user
    ///     that the transaction has completed.
    /// </summary>
    public string? WalletUrl { get; init; }

    /// <summary>
    ///     Defaults to en. Language code specified using ISO 639-1.
    /// </summary>
    public string? Lang { get; init; }

    /// <summary>
    ///     A URL that the anchor should POST a JSON message to when the
    ///     status property of the transaction created as a result of this request
    ///     changes.
    /// </summary>
    public string? OnChangeCallback { get; init; }

    /// <summary>
    ///     The amount of the asset the user would like to deposit with
    ///     the anchor. This field may be necessary for the anchor to determine
    ///     what KYC information is necessary to collect.
    /// </summary>
    public string? Amount { get; init; }

    /// <summary>
    ///     The ISO 3166-1 alpha-3 code of the user's current address.
    ///     This field may be necessary for the anchor to determine what KYC
    ///     information is necessary to collect.
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    ///     True if the client supports receiving deposit transactions as
    ///     a claimable balance, false otherwise.
    /// </summary>
    public string? ClaimableBalanceSupported { get; init; }

    /// <summary>
    ///     Id of an off-chain account (managed by the anchor) associated
    ///     with this user's Stellar account (identified by the JWT's sub field).
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    ///     Id of the chosen location to drop off cash.
    /// </summary>
    public string? LocationId { get; init; }

    /// <summary>
    ///     Can be used to provide extra fields for the request.
    ///     E.g. required fields from the /info endpoint that are not covered by
    ///     the standard parameters.
    /// </summary>
    public Dictionary<string, string>? ExtraFields { get; init; }

    /// <summary>
    ///     JWT previously received from the anchor via the SEP-10 authentication flow.
    /// </summary>
    public string? Jwt { get; init; }
}

