using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the liquidity_pool_withdrew effect response.
///     This effect occurs when assets are withdrawn from a liquidity pool.
/// </summary>
public sealed class LiquidityPoolWithdrewEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 91;

    /// <summary>
    ///     The liquidity pool from which assets were withdrawn.
    /// </summary>
    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool? LiquidityPool { get; init; }

    /// <summary>
    ///     The amounts of each reserve asset received.
    /// </summary>
    [JsonPropertyName("reserves_received")]
    public AssetAmount[]? ReservesReceived { get; init; }

    /// <summary>
    ///     The number of pool shares redeemed.
    /// </summary>
    [JsonPropertyName("shares_redeemed")]
    public string? SharesRedeemed { get; init; }
}