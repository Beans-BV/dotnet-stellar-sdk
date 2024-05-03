using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolWithdrewEffectResponse : EffectResponse
{
    public override int TypeId => 91;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonProperty(PropertyName = "reserves_received")]
    public AssetAmount[] ReservesReceived { get; init; }

    [JsonProperty(PropertyName = "shares_redeemed")]
    public string SharesRedeemed { get; init; }
}