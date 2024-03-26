using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

public class LiquidityPoolWithdrewEffectResponse : EffectResponse
{
    public LiquidityPoolWithdrewEffectResponse()
    {
    }

    public LiquidityPoolWithdrewEffectResponse(LiquidityPool liquidityPool, AssetAmount[] reservesReceived,
        string sharesRedeemed)
    {
        LiquidityPool = liquidityPool;
        ReservesReceived = reservesReceived;
        SharesRedeemed = sharesRedeemed;
    }

    public override int TypeId => 91;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool LiquidityPool { get; private set; }

    [JsonProperty(PropertyName = "reserves_received")]
    public AssetAmount[] ReservesReceived { get; private set; }

    [JsonProperty(PropertyName = "shares_redeemed")]
    public string SharesRedeemed { get; private set; }
}