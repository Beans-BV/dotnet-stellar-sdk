using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.effects;

public class LiquidityPoolDepositedEffectResponse : EffectResponse
{
    public LiquidityPoolDepositedEffectResponse()
    {
    }

    public LiquidityPoolDepositedEffectResponse(LiquidityPool liquidityPool, AssetAmount[] reservesDeposited,
        string sharesReceived)
    {
        LiquidityPool = liquidityPool;
        ReservesDeposited = reservesDeposited;
        SharesReceived = sharesReceived;
    }

    public override int TypeId => 90;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool LiquidityPool { get; private set; }

    [JsonProperty(PropertyName = "reserves_deposited")]
    public AssetAmount[] ReservesDeposited { get; private set; }

    [JsonProperty(PropertyName = "shares_received")]
    public string SharesReceived { get; private set; }
}