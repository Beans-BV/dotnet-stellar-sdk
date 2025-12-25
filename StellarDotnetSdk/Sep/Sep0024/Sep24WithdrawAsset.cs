using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Information about a specific asset available for withdrawal.
///     Contains configuration details for withdrawing an asset, including withdrawal limits and fee structure.
///     Returned as part of the /info endpoint response.
/// </summary>
public class Sep24WithdrawAsset : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24WithdrawAsset" /> class.
    /// </summary>
    /// <param name="enabled">True if withdrawal for this asset is supported by the anchor.</param>
    /// <param name="minAmount">Minimum amount that can be withdrawn. No limit if not specified.</param>
    /// <param name="maxAmount">Maximum amount that can be withdrawn. No limit if not specified.</param>
    /// <param name="feeFixed">Fixed (base) fee for withdrawal in units of the withdrawn asset.</param>
    /// <param name="feePercent">Percentage fee for withdrawal in percentage points.</param>
    /// <param name="feeMinimum">Minimum fee in units of the withdrawn asset.</param>
    [JsonConstructor]
    public Sep24WithdrawAsset(
        bool enabled,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        decimal? feeFixed = null,
        decimal? feePercent = null,
        decimal? feeMinimum = null)
    {
        Enabled = enabled;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        FeeFixed = feeFixed;
        FeePercent = feePercent;
        FeeMinimum = feeMinimum;
    }

    /// <summary>
    ///     Gets a value indicating whether withdrawal for this asset is supported by the anchor.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; }

    /// <summary>
    ///     Gets the minimum amount that can be withdrawn.
    ///     No limit if not specified.
    /// </summary>
    [JsonPropertyName("min_amount")]
    public decimal? MinAmount { get; }

    /// <summary>
    ///     Gets the maximum amount that can be withdrawn.
    ///     No limit if not specified.
    /// </summary>
    [JsonPropertyName("max_amount")]
    public decimal? MaxAmount { get; }

    /// <summary>
    ///     Gets the fixed (base) fee for withdrawal in units of the withdrawn asset.
    ///     This is in addition to any feePercent.
    ///     Omitted if there is no fee or the fee schedule is complex (use /fee endpoint).
    /// </summary>
    [JsonPropertyName("fee_fixed")]
    public decimal? FeeFixed { get; }

    /// <summary>
    ///     Gets the percentage fee for withdrawal in percentage points.
    ///     This is in addition to any feeFixed.
    ///     Omitted if there is no fee or the fee schedule is complex (use /fee endpoint).
    /// </summary>
    [JsonPropertyName("fee_percent")]
    public decimal? FeePercent { get; }

    /// <summary>
    ///     Gets the minimum fee in units of the withdrawn asset.
    /// </summary>
    [JsonPropertyName("fee_minimum")]
    public decimal? FeeMinimum { get; }
}

