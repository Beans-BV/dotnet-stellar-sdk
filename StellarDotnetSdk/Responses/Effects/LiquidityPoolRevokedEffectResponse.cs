using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

public class LiquidityPoolRevokedEffectResponse : EffectResponse
{
    public override int TypeId => 95;

    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    [JsonPropertyName("reserves_revoked")]
    public LiquidityPoolClaimableAssetAmount[] ReservesRevoked { get; init; }

    [JsonPropertyName("shares_revoked")]
    public string SharesRevoked { get; init; }
}