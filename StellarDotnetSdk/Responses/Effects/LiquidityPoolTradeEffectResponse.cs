using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
public class LiquidityPoolTradeEffectResponse : EffectResponse
{
    public override int TypeId => 92;

    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonPropertyName("sold")] public AssetAmount Sold { get; init; }

    [JsonPropertyName("bought")]
    public AssetAmount Bought { get; init; }
}