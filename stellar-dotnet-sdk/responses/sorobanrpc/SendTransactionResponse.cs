﻿using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

/// <summary>
///     Transaction status and network state. The result will include if the transaction was successfully enqueued, and
///     information about the current ledger.
/// </summary>
/// <seealso href="https://soroban.stellar.org/api/methods/sendTransaction" />
[JsonObject]
public class SendTransactionResponse
{
    public enum SendTransactionStatus
    {
        PENDING,
        DUPLICATE,
        TRY_AGAIN_LATER,
        ERROR
    }

    /// <summary>
    ///     (optional) If the transaction status is ERROR, this will be a base64 encoded string of the raw TransactionResult
    ///     XDR struct containing details on why stellar-core rejected the transaction.
    /// </summary>
    public readonly string? ErrorResultXdr;

    /// <summary>
    ///     The transaction hash (in an hex-encoded string).
    /// </summary>
    public readonly string? Hash;

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public readonly long? LatestLedger;

    /// <summary>
    ///     The unix timestamp of the close time of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public readonly long? LatestLedgerCloseTime;

    /// <summary>
    ///     The current status of the transaction by hash, one of: ERROR, PENDING, DUPLICATE, TRY_AGAIN_LATER.
    ///     ERROR represents the status value returned by stellar-core when an error occurred from submitting a transaction.
    ///     PENDING represents the status value returned by stellar-core when a transaction has been accepted for processing.
    ///     DUPLICATE represents the status value returned by stellar-core when a submitted transaction is a duplicate.
    ///     TRY_AGAIN_LATER represents the status value returned by stellar-core when a submitted transaction was not included
    ///     in the previous 4 ledgers and get banned for being added in the next few ledgers.
    /// </summary>
    public readonly SendTransactionStatus Status;

    public SendTransactionResponse(string? errorResultXdr, string? hash, long? latestLedger,
        long? latestLedgerCloseTime, SendTransactionStatus status)
    {
        ErrorResultXdr = errorResultXdr;
        Hash = hash;
        LatestLedger = latestLedger;
        LatestLedgerCloseTime = latestLedgerCloseTime;
        Status = status;
    }
}