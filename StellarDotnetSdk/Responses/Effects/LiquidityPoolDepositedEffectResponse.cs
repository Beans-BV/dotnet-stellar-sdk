using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the liquidity_pool_deposited effect response.
///     This effect occurs when assets are deposited into a liquidity pool.
/// </summary>
public sealed class LiquidityPoolDepositedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 90;

    /// <summary>
    ///     The liquidity pool that received the deposit.
    /// </summary>
    [JsonPropertyName("liquidity_pool")]
    public required LiquidityPool.LiquidityPool LiquidityPool { get; init; }

    /// <summary>
    ///     The amounts of each reserve asset deposited.
    /// </summary>
    [JsonPropertyName("reserves_deposited")]
    public required AssetAmount[] ReservesDeposited { get; init; }

    /// <summary>
    ///     The number of pool shares received by the depositor.
    /// </summary>
    [JsonPropertyName("shares_received")]
    public required string SharesReceived { get; init; }
}