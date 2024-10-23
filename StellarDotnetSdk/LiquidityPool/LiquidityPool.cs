using Newtonsoft.Json;
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

    [JsonProperty(PropertyName = "id")] public LiquidityPoolID ID { get; }

    [JsonProperty(PropertyName = "fee_bp")]
    public int FeeBP { get; }

    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    [JsonProperty(PropertyName = "type")]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; }

    [JsonProperty(PropertyName = "total_trustlines")]
    public int TotalTrustlines { get; }

    [JsonProperty(PropertyName = "total_shares")]
    public string TotalShares { get; }

    [JsonProperty(PropertyName = "reserves")]
    public AssetAmount[] Reserves { get; }
}