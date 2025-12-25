using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for a withdrawal asset supported by the anchor.
///     Contains all the details about how withdrawals work for a specific asset,
///     including whether it's enabled, authentication requirements, fee structure,
///     transaction limits, and supported withdrawal types with their required fields.
/// </summary>
public sealed class WithdrawAsset
{
    /// <summary>
    ///     True if SEP-6 withdrawal for this asset is supported.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>
    ///     Optional. True if client must be authenticated before accessing
    ///     the withdraw endpoint for this asset. False if not specified.
    /// </summary>
    [JsonPropertyName("authentication_required")]
    public bool? AuthenticationRequired { get; init; }

    /// <summary>
    ///     Optional fixed (flat) fee for withdraw, in units of the Stellar asset.
    ///     Null if there is no fee or the fee schedule is complex.
    /// </summary>
    [JsonPropertyName("fee_fixed")]
    public decimal? FeeFixed { get; init; }

    /// <summary>
    ///     Optional percentage fee for withdraw, in percentage points of the
    ///     Stellar asset. Null if there is no fee or the fee schedule is complex.
    /// </summary>
    [JsonPropertyName("fee_percent")]
    public decimal? FeePercent { get; init; }

    /// <summary>
    ///     Optional minimum amount. No limit if not specified.
    /// </summary>
    [JsonPropertyName("min_amount")]
    public decimal? MinAmount { get; init; }

    /// <summary>
    ///     Optional maximum amount. No limit if not specified.
    /// </summary>
    [JsonPropertyName("max_amount")]
    public decimal? MaxAmount { get; init; }

    /// <summary>
    ///     A field with each type of withdrawal supported for that asset as a key.
    ///     Each type can specify a fields object explaining what fields
    ///     are needed and what they do. Anchors are encouraged to use SEP-9
    ///     financial account fields, but can also define custom fields if necessary.
    /// </summary>
    [JsonPropertyName("types")]
    public Dictionary<string, Dictionary<string, AnchorField>?>? Types { get; init; }
}