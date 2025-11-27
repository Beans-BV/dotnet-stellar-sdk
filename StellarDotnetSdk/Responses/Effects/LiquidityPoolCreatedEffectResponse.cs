using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the liquidity_pool_created effect response.
///     This effect occurs when a new liquidity pool is created.
/// </summary>
public sealed class LiquidityPoolCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 93;

    /// <summary>
    ///     The liquidity pool that was created.
    /// </summary>
    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool? LiquidityPool { get; init; }
}