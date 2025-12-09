using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a liquidity_pool_withdraw operation response.
///     Withdraws assets from a liquidity pool by burning pool shares.
/// </summary>
public class LiquidityPoolWithdrawOperationResponse : OperationResponse
{
    public override int TypeId => 23;

    /// <summary>
    ///     The ID of the liquidity pool.
    /// </summary>
    [JsonPropertyName("liquidity_pool_id")]
    public required LiquidityPoolId LiquidityPoolId { get; init; }

    /// <summary>
    ///     The minimum amount of each reserve the withdrawer is willing to accept.
    /// </summary>
    [JsonPropertyName("reserves_min")]
    public required List<Reserve> ReservesMin { get; init; }

    /// <summary>
    ///     The actual amount of each reserve that was received.
    /// </summary>
    [JsonPropertyName("reserves_received")]
    public required List<Reserve> ReservesReceived { get; init; }

    /// <summary>
    ///     The number of pool shares burned in exchange for the withdrawn assets.
    /// </summary>
    [JsonPropertyName("shares")]
    public required string Shares { get; init; }
}