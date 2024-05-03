using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

#nullable disable

public class FeeStatsResponse : Response
{
    [JsonProperty(PropertyName = "ledger_capacity_usage")]
    public decimal LedgerCapacityUsage { get; init; }

    [JsonProperty(PropertyName = "last_ledger_base_fee")]
    public long LastLedgerBaseFee { get; init; }

    [JsonProperty(PropertyName = "last_ledger")]
    public uint LastLedger { get; init; }

    [JsonProperty(PropertyName = "fee_charged")]
    public FeeStatsResponseData FeeCharged { get; init; }

    [JsonProperty(PropertyName = "max_fee")]
    public FeeStatsResponseData MaxFee { get; init; }

    public class FeeStatsResponseData
    {
        [JsonProperty(PropertyName = "max")] public long Max { get; init; }

        [JsonProperty(PropertyName = "min")] public long Min { get; init; }

        [JsonProperty(PropertyName = "mode")] public long Mode { get; init; }

        [JsonProperty(PropertyName = "p10")] public long P10 { get; init; }

        [JsonProperty(PropertyName = "p20")] public long P20 { get; init; }

        [JsonProperty(PropertyName = "p30")] public long P30 { get; init; }

        [JsonProperty(PropertyName = "p40")] public long P40 { get; init; }

        [JsonProperty(PropertyName = "p50")] public long P50 { get; init; }

        [JsonProperty(PropertyName = "p60")] public long P60 { get; init; }

        [JsonProperty(PropertyName = "p70")] public long P70 { get; init; }

        [JsonProperty(PropertyName = "p80")] public long P80 { get; init; }

        [JsonProperty(PropertyName = "p90")] public long P90 { get; init; }

        [JsonProperty(PropertyName = "p95")] public long P95 { get; init; }

        [JsonProperty(PropertyName = "p99")] public long P99 { get; init; }
    }
}