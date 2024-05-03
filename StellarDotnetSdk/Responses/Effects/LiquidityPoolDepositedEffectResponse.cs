using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolDepositedEffectResponse : EffectResponse
{
    public override int TypeId => 90;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonProperty(PropertyName = "reserves_deposited")]
    public AssetAmount[] ReservesDeposited { get; init; }

    [JsonProperty(PropertyName = "shares_received")]
    public string SharesReceived { get; init; }
}