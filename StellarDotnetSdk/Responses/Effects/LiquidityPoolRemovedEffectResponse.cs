using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

public class LiquidityPoolRemovedEffectResponse : EffectResponse
{
    public LiquidityPoolRemovedEffectResponse()
    {
    }

    public LiquidityPoolRemovedEffectResponse(LiquidityPoolID liquidityPoolID)
    {
        LiquidityPoolID = liquidityPoolID;
    }

    public override int TypeId => 94;

    [JsonProperty(PropertyName = "liquidity_pool_id")]
    public LiquidityPoolID LiquidityPoolID { get; private set; }
}