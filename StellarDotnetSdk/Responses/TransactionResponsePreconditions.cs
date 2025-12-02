using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     A set of transaction preconditions affecting its validity.
/// </summary>
public sealed class TransactionResponsePreconditions
{
    /// <summary>
    ///     Link to the source account of this transaction.
    /// </summary>
    [JsonPropertyName("timebounds")]
    public required TimeBounds TimeBounds { get; init; }

    /// <summary>
    ///     The ledger range for which this transaction is valid, as unsigned 32-bit integers.
    /// </summary>
    // TODO: Add tests when it's clear on how to add ledger bounds to transaction
    [JsonPropertyName("ledger_bounds")]
    public LedgerBounds? LedgerBounds { get; init; }

    /// <summary>
    ///     Containing a positive, signed 64-bit integer representing the lowest source account sequence number for which the transaction is valid.
    /// </summary>
    [JsonPropertyName("min_account_sequence")]
    public long? MinAccountSequence { get; init; }

    /// <summary>
    ///     The minimum duration of time (in seconds as an unsigned 64-bit integer) that must have passed since the source account's sequence number changed for the transaction to be valid.
    /// </summary>
    [JsonPropertyName("min_account_sequence_age")]
    public ulong? MinAccountSequenceAge { get; init; }

    /// <summary>
    ///     An unsigned 32-bit integer representing the minimum number of ledgers that must have closed since the source account's sequence number changed for the transaction to be valid.
    /// </summary>
    [JsonPropertyName("min_account_sequence_ledger_gap")]
    public uint? MinAccountSequenceLedgerGap { get; init; }

    /// <summary>
    ///     The list of up to two additional signers that must have corresponding signatures for this transaction to be valid.
    /// </summary>
    [JsonPropertyName("extra_signers")]
    public string[]? ExtraSigners { get; init; }
}

/// <summary>
///     The time range for which this transaction is valid, with bounds as unsigned 64-bit UNIX timestamps.
/// </summary>
public sealed class TimeBounds
{
    /// <summary>
    ///     The lower bound.
    /// </summary>
    [JsonPropertyName("min_time")]
    public required string MinTime { get; init; }

    /// <summary>
    ///     The upper bound.
    /// </summary>
    [JsonPropertyName("max_time")]
    public string? MaxTime { get; init; }
}

/// <summary>
///     The ledger range for which this transaction is valid, as unsigned 32-bit integers.
/// </summary>
public sealed class LedgerBounds
{
    /// <summary>
    ///     The lower bound.
    /// </summary>
    [JsonPropertyName("min_ledger")]
    public string? MinLedger { get; init; }

    /// <summary>
    ///     The upper bound.
    /// </summary>
    [JsonPropertyName("max_ledger")]
    public string? MaxLedger { get; init; }
}