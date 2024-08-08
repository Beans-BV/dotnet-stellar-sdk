namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Statistics for charged inclusion fees. The inclusion fee statistics are calculated from the inclusion fees that
///     were paid for the transactions to be included onto the ledger. For Soroban transactions and Stellar transactions,
///     they each have their own inclusion fees and own surge pricing. Inclusion fees are used to prevent spam and
///     prioritize transactions during network traffic surge.
/// </summary>
public class GetFeeStatsResponse
{
    public GetFeeStatsResponse(InclusionFee? sorobanInclusionFee, InclusionFee? inclusionFee, long latestLedger)
    {
        SorobanInclusionFee = sorobanInclusionFee;
        InclusionFee = inclusionFee;
        LatestLedger = latestLedger;
    }

    /// <summary>
    ///     Inclusion fee distribution statistics for Soroban transactions
    /// </summary>
    public InclusionFee? SorobanInclusionFee { get; }

    /// <summary>
    ///     Fee distribution statistics for Stellar (i.e. non-Soroban) transactions. Statistics are normalized per operation.
    /// </summary>
    public InclusionFee? InclusionFee { get; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long LatestLedger { get; }
}

public class InclusionFee
{
    public InclusionFee(
        string max, string min, string mode,
        string p10, string p20, string p30,
        string p40, string p50, string p60,
        string p70, string p80, string p90,
        string p99, string transactionCount,
        int ledgerCount)
    {
        Max = max;
        Min = min;
        Mode = mode;
        P10 = p10;
        P20 = p20;
        P30 = p30;
        P40 = p40;
        P50 = p50;
        P60 = p60;
        P70 = p70;
        P80 = p80;
        P90 = p90;
        P99 = p99;
        TransactionCount = transactionCount;
        LedgerCount = ledgerCount;
    }

    /// Maximum fee
    public string Max { get; }

    /// Minimum fee
    public string Min { get; }

    /// Fee value which occurs the most often
    public string Mode { get; }

    /// 10th nearest-rank fee percentile
    public string P10 { get; }

    /// 20th nearest-rank fee percentile
    public string P20 { get; }

    /// 30th nearest-rank fee percentile
    public string P30 { get; }

    /// 40th nearest-rank fee percentile
    public string P40 { get; }

    /// 50th nearest-rank fee percentile
    public string P50 { get; }

    /// 60th nearest-rank fee percentile
    public string P60 { get; }

    /// 70th nearest-rank fee percentile
    public string P70 { get; }

    /// 80th nearest-rank fee percentile
    public string P80 { get; }

    /// 90th nearest-rank fee percentile
    public string P90 { get; }

    // TODO: Re-check if the result contains this property
    // See https://github.com/stellar/stellar-docs/issues/892
    /// 95th nearest-rank fee percentile
    // public string P95 { get; }

    /// 99th nearest-rank fee percentile
    public string P99 { get; }

    /// How many transactions are part of the distribution
    public string TransactionCount { get; }

    /// How many consecutive ledgers form the distribution
    public int LedgerCount { get; }
}