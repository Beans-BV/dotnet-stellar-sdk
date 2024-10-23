using Newtonsoft.Json;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 94;

    [JsonProperty(PropertyName = "liquidity_pool_id")]
    public LiquidityPoolID LiquidityPoolId { get; private set; }
}