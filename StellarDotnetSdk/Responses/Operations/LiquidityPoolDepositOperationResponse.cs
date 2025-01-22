using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
public class LiquidityPoolDepositOperationResponse : OperationResponse
{
    public override int TypeId => 22;

    [JsonPropertyName("liquidity_pool_id")] public LiquidityPoolId LiquidityPoolId { get; init; }

    [JsonPropertyName("reserves_max")] public List<Reserve> ReservesMax { get; init; }

    [JsonPropertyName("min_price")] public string MinPrice { get; init; }

    [JsonPropertyName("max_price")] public string MaxPrice { get; init; }

    [JsonPropertyName("reserves_deposited")] public List<Reserve> ReservesDeposited { get; init; }

    [JsonPropertyName("shares_received")] public string SharesReceived { get; init; }
}