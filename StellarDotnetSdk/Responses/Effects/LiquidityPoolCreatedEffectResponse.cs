using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 93;

    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }
}