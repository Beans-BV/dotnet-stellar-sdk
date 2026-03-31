using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LiquidityPool;

/// <summary>
///     Represents a Stellar liquidity pool, which holds reserves of two assets and allows
///     decentralized trading based on an automated market maker (AMM) algorithm.
/// </summary>
public class LiquidityPool
{
    /// <summary>
    ///     Constructs a <c>LiquidityPool</c> with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the liquidity pool.</param>
    /// <param name="feeBp">The fee charged per trade in basis points.</param>
    /// <param name="type">The type of the liquidity pool (e.g., constant product).</param>
    /// <param name="totalTrustlines">The number of accounts that have trustlines to this pool.</param>
    /// <param name="totalShares">The total number of pool shares outstanding.</param>
    /// <param name="reserves">The current reserves held by the pool for each asset.</param>
    public LiquidityPool(LiquidityPoolId id,
        int feeBp,
        LiquidityPoolType.LiquidityPoolTypeEnum type,
        int totalTrustlines,
        string totalShares,
        AssetAmount[] reserves)
    {
        Id = id;
        FeeBp = feeBp;
        Type = type;
        TotalTrustlines = totalTrustlines;
        TotalShares = totalShares;
        Reserves = reserves;
    }

    /// <summary>
    ///     The unique identifier of the liquidity pool, derived from the SHA-256 hash of its parameters.
    /// </summary>
    [JsonPropertyName("id")]
    public LiquidityPoolId Id { get; }

    /// <summary>
    ///     The fee charged per trade in basis points (1 bp = 0.01%). Currently fixed at 30 bp (0.3%).
    /// </summary>
    [JsonPropertyName("fee_bp")]
    public int FeeBp { get; }

    /// <summary>
    ///     The type of the liquidity pool (currently only constant product is supported).
    /// </summary>
    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; }

    /// <summary>
    ///     The number of accounts that have trustlines to this pool's share asset.
    /// </summary>
    [JsonPropertyName("total_trustlines")]
    public int TotalTrustlines { get; }

    /// <summary>
    ///     The total number of pool shares outstanding, represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("total_shares")]
    public string TotalShares { get; }

    /// <summary>
    ///     The current reserve amounts held by the pool for each of its two assets.
    /// </summary>
    [JsonPropertyName("reserves")]
    public AssetAmount[] Reserves { get; }
}