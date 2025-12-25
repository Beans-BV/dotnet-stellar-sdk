using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for a deposit-exchange asset supported by the anchor.
///     This class represents assets that can be deposited with simultaneous conversion
///     to another asset on the Stellar network. Used in SEP-38 quote-assisted deposit
///     operations where users deposit one asset (e.g., USD) and receive a different
///     asset (e.g., USDC) on Stellar.
/// </summary>
public sealed class DepositExchangeAsset
{
    /// <summary>
    ///     True if SEP-6 deposit-exchange for this asset is supported.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>
    ///     Optional. True if client must be authenticated before accessing the
    ///     deposit-exchange endpoint for this asset. False if not specified.
    /// </summary>
    [JsonPropertyName("authentication_required")]
    public bool? AuthenticationRequired { get; init; }

    /// <summary>
    ///     A list of methods supported by the Anchor for transferring or settling assets.
    ///     For deposits, this specifies the methods available to transfer funds from the
    ///     client's account to the anchor, typically listing options such as WIRE or ACH
    ///     for fiat settlements.
    /// </summary>
    [JsonPropertyName("funding_methods")]
    public IReadOnlyList<string>? FundingMethods { get; init; }

    /// <summary>
    ///     (Deprecated) Accepting personally identifiable information through
    ///     request parameters is a security risk due to web server request logging.
    ///     KYC information should be supplied to the Anchor via SEP-12.
    /// </summary>
    [JsonPropertyName("fields")]
    public Dictionary<string, AnchorField>? Fields { get; init; }
}