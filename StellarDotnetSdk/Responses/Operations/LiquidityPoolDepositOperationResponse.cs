using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
public class LiquidityPoolDepositOperationResponse : OperationResponse
{
    public override int TypeId => 22;

    [JsonProperty("liquidity_pool_id")] public LiquidityPoolId LiquidityPoolId { get; init; }

    [JsonProperty("reserves_max")] public List<Reserve> ReservesMax { get; init; }

    [JsonProperty("min_price")] public string MinPrice { get; init; }

    [JsonProperty("max_price")] public string MaxPrice { get; init; }

    [JsonProperty("reserves_deposited")] public List<Reserve> ReservesDeposited { get; init; }

    [JsonProperty("shares_received")] public string SharesReceived { get; init; }
}