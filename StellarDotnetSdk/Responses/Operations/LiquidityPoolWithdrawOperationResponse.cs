using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
public class LiquidityPoolWithdrawOperationResponse : OperationResponse
{
    public override int TypeId => 23;

    [JsonProperty("liquidity_pool_id")] public LiquidityPoolId LiquidityPoolId { get; init; }

    [JsonProperty("reserves_min")] public List<Reserve> ReservesMin { get; init; }

    [JsonProperty("reserves_received")] public List<Reserve> ReservesReceived { get; init; }

    [JsonProperty("shares")] public string Shares { get; init; }
}