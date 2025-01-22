using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 94;

    [JsonPropertyName("liquidity_pool_id")]
    public LiquidityPoolId LiquidityPoolId { get; init; }
}