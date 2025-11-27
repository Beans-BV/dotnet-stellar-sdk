using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the liquidity_pool_removed effect response.
///     This effect occurs when a liquidity pool is removed.
/// </summary>
public sealed class LiquidityPoolRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 94;

    /// <summary>
    ///     The ID of the liquidity pool that was removed.
    /// </summary>
    [JsonPropertyName("liquidity_pool_id")]
    public LiquidityPoolId? LiquidityPoolId { get; init; }
}