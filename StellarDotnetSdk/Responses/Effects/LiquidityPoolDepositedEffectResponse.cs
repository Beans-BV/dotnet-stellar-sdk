using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolDepositedEffectResponse : EffectResponse
{
    public override int TypeId => 90;

    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonPropertyName("reserves_deposited")]
    public AssetAmount[] ReservesDeposited { get; init; }

    [JsonPropertyName("shares_received")]
    public string SharesReceived { get; init; }
}