using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

public class LiquidityPoolCreatedEffectResponse : EffectResponse
{
    public LiquidityPoolCreatedEffectResponse()
    {
    }

    public LiquidityPoolCreatedEffectResponse(LiquidityPool liquidityPool)
    {
        LiquidityPool = liquidityPool;
    }

    public override int TypeId => 93;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool LiquidityPool { get; private set; }
}