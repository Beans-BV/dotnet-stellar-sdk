using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the liquidity_pool_revoked effect response.
///     This effect occurs when pool shares are revoked due to clawback.
/// </summary>
public sealed class LiquidityPoolRevokedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 95;

    /// <summary>
    ///     The liquidity pool that had shares revoked.
    /// </summary>
    [JsonPropertyName("liquidity_pool")]
    public required LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    /// <summary>
    ///     The amounts of each reserve asset revoked.
    /// </summary>
    [JsonPropertyName("reserves_revoked")]
    public required LiquidityPoolClaimableAssetAmount[] ReservesRevoked { get; init; }

    /// <summary>
    ///     The number of pool shares revoked.
    /// </summary>
    [JsonPropertyName("shares_revoked")]
    public required string SharesRevoked { get; init; }
}