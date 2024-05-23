using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LiquidityPool;

public class LiquidityPool
{
    public LiquidityPool(LiquidityPoolID id,
        int feeBp,
        LiquidityPoolType.LiquidityPoolTypeEnum type,
        int totalTrustlines,
        string totalShares,
        AssetAmount[] reserves)
    {
        ID = id;
        FeeBP = feeBp;
        Type = type;
        TotalTrustlines = totalTrustlines;
        TotalShares = totalShares;
        Reserves = reserves;
    }

    [JsonPropertyName("id")] public LiquidityPoolID ID { get; }

    [JsonPropertyName("fee_bp")]
    public int FeeBP { get; }

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