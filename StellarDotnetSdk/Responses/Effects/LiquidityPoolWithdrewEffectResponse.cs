using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolWithdrewEffectResponse : EffectResponse
{
    public override int TypeId => 91;

    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonPropertyName("reserves_received")]
    public AssetAmount[] ReservesReceived { get; init; }

    [JsonPropertyName("shares_redeemed")]
    public string SharesRedeemed { get; init; }
}