using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for a withdrawal-exchange asset supported by the anchor.
///     This class represents assets that can be withdrawn with simultaneous conversion
///     from another asset on the Stellar network. Used in SEP-38 quote-assisted withdrawal
///     operations where users send one asset (e.g., USDC) on Stellar and receive a
///     different asset (e.g., USD) off-chain.
/// </summary>
public sealed class WithdrawExchangeAsset
{
    /// <summary>
    ///     True if SEP-6 withdrawal-exchange for this asset is supported.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>
    ///     Optional. True if client must be authenticated before accessing
    ///     the withdraw-exchange endpoint for this asset. False if not specified.
    /// </summary>
    [JsonPropertyName("authentication_required")]
    public bool? AuthenticationRequired { get; init; }

    /// <summary>
    ///     A field with each type of withdrawal supported for that asset as a key.
    ///     Each type can specify a fields object explaining what fields
    ///     are needed and what they do. Anchors are encouraged to use SEP-9
    ///     financial account fields, but can also define custom fields if necessary.
    /// </summary>
    [JsonPropertyName("types")]
    public Dictionary<string, Dictionary<string, AnchorField>?>? Types { get; init; }
}