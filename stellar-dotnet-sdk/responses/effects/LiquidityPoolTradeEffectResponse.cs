﻿using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.effects;

public class LiquidityPoolTradeEffectResponse : EffectResponse
{
    public LiquidityPoolTradeEffectResponse()
    {
    }

    public LiquidityPoolTradeEffectResponse(LiquidityPool liquidityPool, AssetAmount sold, AssetAmount bought)
    {
        LiquidityPool = liquidityPool;
        Sold = sold;
        Bought = bought;
    }

    public override int TypeId => 92;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool LiquidityPool { get; private set; }

    [JsonProperty(PropertyName = "sold")] public AssetAmount Sold { get; private set; }

    [JsonProperty(PropertyName = "bought")]
    public AssetAmount Bought { get; private set; }
}