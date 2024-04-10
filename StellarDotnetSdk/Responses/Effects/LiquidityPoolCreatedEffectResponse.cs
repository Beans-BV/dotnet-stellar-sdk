using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 93;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }
}