using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

#nullable disable

public class FeeStatsResponse : Response
{
    [JsonPropertyName("ledger_capacity_usage")]
    public decimal LedgerCapacityUsage { get; init; }

    [JsonPropertyName("last_ledger_base_fee")]
    public long LastLedgerBaseFee { get; init; }

    [JsonPropertyName("last_ledger")]
    public uint LastLedger { get; init; }

    [JsonPropertyName("fee_charged")]
    public FeeStatsResponseData FeeCharged { get; init; }

    [JsonPropertyName("max_fee")]
    public FeeStatsResponseData MaxFee { get; init; }

    public class FeeStatsResponseData
    {
        [JsonPropertyName("max")] public long Max { get; init; }

        [JsonPropertyName("min")] public long Min { get; init; }

        [JsonPropertyName("mode")] public long Mode { get; init; }

        [JsonPropertyName("p10")] public long P10 { get; init; }

        [JsonPropertyName("p20")] public long P20 { get; init; }

        [JsonPropertyName("p30")] public long P30 { get; init; }

        [JsonPropertyName("p40")] public long P40 { get; init; }

        [JsonPropertyName("p50")] public long P50 { get; init; }

        [JsonPropertyName("p60")] public long P60 { get; init; }

        [JsonPropertyName("p70")] public long P70 { get; init; }

        [JsonPropertyName("p80")] public long P80 { get; init; }

        [JsonPropertyName("p90")] public long P90 { get; init; }

        [JsonPropertyName("p95")] public long P95 { get; init; }

        [JsonPropertyName("p99")] public long P99 { get; init; }
    }
}