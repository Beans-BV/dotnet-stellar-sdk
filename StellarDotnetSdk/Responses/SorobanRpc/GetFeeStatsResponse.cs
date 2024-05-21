namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Statistics for charged inclusion fees. The inclusion fee statistics are calculated from the inclusion fees that
///     were paid for the transactions to be included onto the ledger. For Soroban transactions and Stellar transactions,
///     they each have their own inclusion fees and own surge pricing. Inclusion fees are used to prevent spam and
///     prioritize transactions during network traffic surge.
/// </summary>
public class GetFeeStatsResponse
{
    /// <summary>
    ///     Inclusion fee distribution statistics for Soroban transactions
    /// </summary>
    public InclusionFee? SorobanInclusionFee { get; init; }

    /// <summary>
    ///     Fee distribution statistics for Stellar (i.e. non-Soroban) transactions. Statistics are normalized per operation.
    /// </summary>
    public InclusionFee? InclusionFee { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedger { get; init; }
}

public class InclusionFee
{
    /// Maximum fee
    public string Max { get; init; }

    /// Minimum fee
    public string Min { get; init; }

    /// Fee value which occurs the most often
    public string Mode { get; init; }

    /// 10th nearest-rank fee percentile
    public string P10 { get; init; }

    /// 20th nearest-rank fee percentile
    public string P20 { get; init; }

    /// 30th nearest-rank fee percentile
    public string P30 { get; init; }

    /// 40th nearest-rank fee percentile
    public string P40 { get; init; }

    /// 50th nearest-rank fee percentile
    public string P50 { get; init; }

    /// 60th nearest-rank fee percentile
    public string P60 { get; init; }

    /// 70th nearest-rank fee percentile
    public string P70 { get; init; }

    /// 80th nearest-rank fee percentile
    public string P80 { get; init; }

    /// 90th nearest-rank fee percentile
    public string P90 { get; init; }

    /// 95th nearest-rank fee percentile
    public string P95 { get; init; }

    /// 99th nearest-rank fee percentile
    public string P99 { get; init; }

    /// How many transactions are part of the distribution
    public string TransactionCount { get; init; }

    /// How many consecutive ledgers form the distribution
    public int LedgerCount { get; init; }
}