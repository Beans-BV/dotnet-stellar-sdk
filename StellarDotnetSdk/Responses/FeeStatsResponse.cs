using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents fee statistics for the Stellar network.
///     Provides information about transaction fees across recent ledgers to help with fee estimation.
/// </summary>
public sealed class FeeStatsResponse : Response
{
    /// <summary>
    ///     The average capacity usage over the last 5 ledgers (0 is no usage, 1.0 is completely full ledgers).
    /// </summary>
    [JsonPropertyName("ledger_capacity_usage")]
    public required string LedgerCapacityUsage { get; init; }

    /// <summary>
    ///     The base fee as defined in the last ledger.
    /// </summary>
    [JsonPropertyName("last_ledger_base_fee")]
    public required long LastLedgerBaseFee { get; init; }

    /// <summary>
    ///     The last ledger's sequence number.
    /// </summary>
    [JsonPropertyName("last_ledger")]
    public required long LastLedger { get; init; }

    /// <summary>
    ///     Information about the fee charged for transactions in the last 5 ledgers.
    /// </summary>
    [JsonPropertyName("fee_charged")]
    public required FeeStatsResponseData FeeCharged { get; init; }

    /// <summary>
    ///     Information about max fee bid for transactions over the last 5 ledgers.
    /// </summary>
    [JsonPropertyName("max_fee")]
    public required FeeStatsResponseData MaxFee { get; init; }

    /// <summary>
    ///     Contains statistical data about fees including minimum, maximum, mode, and percentiles.
    /// </summary>
    public sealed class FeeStatsResponseData
    {
        /// <summary>
        ///     The maximum fee over the last 5 ledgers.
        /// </summary>
        // TODO: Report an issue as this is not included in the <a href="https://developers.stellar.org/docs/data/apis/horizon/api-reference/aggregations/fee-stats/object">official docs</a>
        [JsonPropertyName("max")]
        public required long Max { get; init; }

        /// <summary>
        ///     The minimum fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("min")]
        public required long Min { get; init; }

        /// <summary>
        ///     The mode fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("mode")]
        public required long Mode { get; init; }

        /// <summary>
        ///     The 10th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p10")]
        public required long P10 { get; init; }

        /// <summary>
        ///     The 20th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p20")]
        public required long P20 { get; init; }

        /// <summary>
        ///     The 30th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p30")]
        public required long P30 { get; init; }

        /// <summary>
        ///     The 40th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p40")]
        public required long P40 { get; init; }

        /// <summary>
        ///     The 50th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p50")]
        public required long P50 { get; init; }

        /// <summary>
        ///     The 60th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p60")]
        public required long P60 { get; init; }

        /// <summary>
        ///     The 70th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p70")]
        public required long P70 { get; init; }

        /// <summary>
        ///     The 80th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p80")]
        public required long P80 { get; init; }

        /// <summary>
        ///     The 90th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p90")]
        public required long P90 { get; init; }

        /// <summary>
        ///     The 95th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p95")]
        public required long P95 { get; init; }

        /// <summary>
        ///     The 99th max percentile fee over the last 5 ledgers.
        /// </summary>
        [JsonPropertyName("p99")]
        public required long P99 { get; init; }
    }
}