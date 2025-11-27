using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the liquidity_pool_trade effect response.
///     This effect occurs when a trade is executed against a liquidity pool.
/// </summary>
public sealed class LiquidityPoolTradeEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 92;

    /// <summary>
    ///     The liquidity pool involved in the trade.
    /// </summary>
    [JsonPropertyName("liquidity_pool")]
    public LiquidityPool.LiquidityPool? LiquidityPool { get; init; }

    /// <summary>
    ///     The asset and amount sold in the trade.
    /// </summary>
    [JsonPropertyName("sold")]
    public AssetAmount? Sold { get; init; }

    /// <summary>
    ///     The asset and amount bought in the trade.
    /// </summary>
    [JsonPropertyName("bought")]
    public AssetAmount? Bought { get; init; }
}