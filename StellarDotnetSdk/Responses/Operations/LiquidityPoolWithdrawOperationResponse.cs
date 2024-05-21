using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
public class LiquidityPoolWithdrawOperationResponse : OperationResponse
{
    public override int TypeId => 23;

    [JsonPropertyName("liquidity_pool_id")] public LiquidityPoolID LiquidityPoolID { get; init; }

    [JsonPropertyName("reserves_min")] public List<Reserve> ReservesMin { get; init; }

    [JsonPropertyName("reserves_received")] public List<Reserve> ReservesReceived { get; init; }

    [JsonPropertyName("shares")] public string Shares { get; init; }
}