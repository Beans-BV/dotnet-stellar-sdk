using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;

public class LiquidityPoolRevokedEffectResponse : EffectResponse
{
    public LiquidityPoolRevokedEffectResponse()
    {
    }

    public LiquidityPoolRevokedEffectResponse(LiquidityPool liquidityPool,
        LiquidityPoolClaimableAssetAmount[] reservesRevoked, string sharesRevoked)
    {
        LiquidityPool = liquidityPool;
        ReservesRevoked = reservesRevoked;
        SharesRevoked = sharesRevoked;
    }

    public override int TypeId => 95;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool LiquidityPool { get; private set; }

    [JsonProperty(PropertyName = "reserves_revoked")]
    public LiquidityPoolClaimableAssetAmount[] ReservesRevoked { get; private set; }

    [JsonProperty(PropertyName = "shares_revoked")]
    public string SharesRevoked { get; private set; }
}