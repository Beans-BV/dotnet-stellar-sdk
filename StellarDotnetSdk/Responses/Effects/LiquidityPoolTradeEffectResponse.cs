using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
public class LiquidityPoolTradeEffectResponse : EffectResponse
{
    public override int TypeId => 92;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonProperty(PropertyName = "sold")] public AssetAmount Sold { get; init; }

    [JsonProperty(PropertyName = "bought")]
    public AssetAmount Bought { get; init; }
}