﻿using Newtonsoft.Json;
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
    [JsonProperty(PropertyName = "type")]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; }

    [JsonProperty(PropertyName = "total_trustlines")]
    public int TotalTrustlines { get; }

    [JsonProperty(PropertyName = "total_shares")]
    public string TotalShares { get; }

    [JsonProperty(PropertyName = "reserves")]
    public AssetAmount[] Reserves { get; }
}