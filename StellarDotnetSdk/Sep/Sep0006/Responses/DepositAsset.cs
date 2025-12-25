using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Configuration for a deposit asset supported by the anchor.
///     Contains all the details about how deposits work for a specific asset,
///     including whether it's enabled, authentication requirements, fee structure,
///     transaction limits, and any additional fields required.
/// </summary>
public sealed class DepositAsset
{
    /// <summary>
    ///     True if SEP-6 deposit for this asset is supported.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>
    ///     Optional. True if client must be authenticated before accessing the
    ///     deposit endpoint for this asset. False if not specified.
    /// </summary>
    [JsonPropertyName("authentication_required")]
    public bool? AuthenticationRequired { get; init; }

    /// <summary>
    ///     Optional fixed (flat) fee for deposit, in units of the Stellar asset.
    ///     Null if there is no fee or the fee schedule is complex.
    /// </summary>
    [JsonPropertyName("fee_fixed")]
    public decimal? FeeFixed { get; init; }

    /// <summary>
    ///     Optional percentage fee for deposit, in percentage points of the Stellar
    ///     asset. Null if there is no fee or the fee schedule is complex.
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
    ///     (Deprecated) Accepting personally identifiable information through
    ///     request parameters is a security risk due to web server request logging.
    ///     KYC information should be supplied to the Anchor via SEP-12.
    /// </summary>
    [JsonPropertyName("fields")]
    public Dictionary<string, AnchorField>? Fields { get; init; }
}

