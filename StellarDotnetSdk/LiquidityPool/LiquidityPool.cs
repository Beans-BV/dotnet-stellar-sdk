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

    [JsonPropertyName("id")]
    public LiquidityPoolId Id { get; }

    [JsonPropertyName("fee_bp")]
    public int FeeBp { get; }

    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; }

    [JsonPropertyName("total_trustlines")]
    public int TotalTrustlines { get; }

    [JsonPropertyName("total_shares")]
    public string TotalShares { get; }

    [JsonPropertyName("reserves")]
    public AssetAmount[] Reserves { get; }
}