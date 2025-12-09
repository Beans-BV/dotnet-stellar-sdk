using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a liquidity_pool_deposit operation response.
///     Deposits assets into a liquidity pool in exchange for pool shares.
/// </summary>
public class LiquidityPoolDepositOperationResponse : OperationResponse
{
    public override int TypeId => 22;

    /// <summary>
    ///     The ID of the liquidity pool.
    /// </summary>
    [JsonPropertyName("liquidity_pool_id")]
    public required LiquidityPoolId LiquidityPoolId { get; init; }

    /// <summary>
    ///     The maximum amount of each reserve the depositor is willing to deposit.
    /// </summary>
    [JsonPropertyName("reserves_max")]
    public required List<Reserve> ReservesMax { get; init; }

    /// <summary>
    ///     The minimum price (as a ratio) the depositor is willing to accept.
    /// </summary>
    [JsonPropertyName("min_price")]
    public required string MinPrice { get; init; }

    /// <summary>
    ///     The maximum price (as a ratio) the depositor is willing to accept.
    /// </summary>
    [JsonPropertyName("max_price")]
    public required string MaxPrice { get; init; }

    /// <summary>
    ///     The actual amount of each reserve that was deposited.
    /// </summary>
    [JsonPropertyName("reserves_deposited")]
    public required List<Reserve> ReservesDeposited { get; init; }

    /// <summary>
    ///     The number of pool shares received in exchange for the deposit.
    /// </summary>
    [JsonPropertyName("shares_received")]
    public required string SharesReceived { get; init; }
}