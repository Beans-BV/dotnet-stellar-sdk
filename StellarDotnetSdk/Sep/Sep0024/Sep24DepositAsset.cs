using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Information about a specific asset available for deposit.
///     Contains configuration details for depositing an asset, including deposit limits and fee structure.
///     Returned as part of the /info endpoint response.
/// </summary>
public class Sep24DepositAsset : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24DepositAsset" /> class.
    /// </summary>
    /// <param name="enabled">True if deposit for this asset is supported by the anchor.</param>
    /// <param name="minAmount">Minimum amount that can be deposited. No limit if not specified.</param>
    /// <param name="maxAmount">Maximum amount that can be deposited. No limit if not specified.</param>
    /// <param name="feeFixed">Fixed (base) fee for deposit in units of the deposited asset.</param>
    /// <param name="feePercent">Percentage fee for deposit in percentage points.</param>
    /// <param name="feeMinimum">Minimum fee in units of the deposited asset.</param>
    [JsonConstructor]
    public Sep24DepositAsset(
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
    ///     Gets a value indicating whether deposit for this asset is supported by the anchor.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; }

    /// <summary>
    ///     Gets the minimum amount that can be deposited.
    ///     No limit if not specified.
    /// </summary>
    [JsonPropertyName("min_amount")]
    public decimal? MinAmount { get; }

    /// <summary>
    ///     Gets the maximum amount that can be deposited.
    ///     No limit if not specified.
    /// </summary>
    [JsonPropertyName("max_amount")]
    public decimal? MaxAmount { get; }

    /// <summary>
    ///     Gets the fixed (base) fee for deposit in units of the deposited asset.
    ///     This is in addition to any feePercent.
    ///     Omitted if there is no fee or the fee schedule is complex (use /fee endpoint).
    /// </summary>
    [JsonPropertyName("fee_fixed")]
    public decimal? FeeFixed { get; }

    /// <summary>
    ///     Gets the percentage fee for deposit in percentage points.
    ///     This is in addition to any feeFixed.
    ///     Omitted if there is no fee or the fee schedule is complex (use /fee endpoint).
    /// </summary>
    [JsonPropertyName("fee_percent")]
    public decimal? FeePercent { get; }

    /// <summary>
    ///     Gets the minimum fee in units of the deposited asset.
    /// </summary>
    [JsonPropertyName("fee_minimum")]
    public decimal? FeeMinimum { get; }
}

