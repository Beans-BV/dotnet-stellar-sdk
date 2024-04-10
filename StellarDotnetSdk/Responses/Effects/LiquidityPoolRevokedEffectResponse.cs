using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolRevokedEffectResponse : EffectResponse
{
    public override int TypeId => 95;

    [JsonProperty(PropertyName = "liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonProperty(PropertyName = "reserves_revoked")]
    public LiquidityPoolClaimableAssetAmount[] ReservesRevoked { get; init; }

    [JsonProperty(PropertyName = "shares_revoked")]
    public string SharesRevoked { get; init; }
}