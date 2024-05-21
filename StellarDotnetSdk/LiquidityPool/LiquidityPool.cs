using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LiquidityPool;

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

    [JsonProperty(PropertyName = "id")] public LiquidityPoolId Id { get; }

    [JsonProperty(PropertyName = "fee_bp")]
    public int FeeBp { get; }

    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    [JsonPropertyName("type")]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; }

    [JsonPropertyName("total_trustlines")]
    public int TotalTrustlines { get; }

    [JsonPropertyName("total_shares")]
    public string TotalShares { get; }

    [JsonPropertyName("reserves")]
    public AssetAmount[] Reserves { get; }
}