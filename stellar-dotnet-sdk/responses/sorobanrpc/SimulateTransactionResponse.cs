﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

/// <summary>
///     <para>
///         The response will include the anticipated affects the given transaction will have on the network. Additionally,
///         information needed to build, sign, and actually submit the transaction will be provided.
///     </para>
///     See https://soroban.stellar.org/api/methods/simulateTransaction/
/// </summary>
[JsonObject]
public class SimulateTransactionResponse
{
    private readonly string? _transactionData;

    public SimulateTransactionResponse(string? transactionData, string? error, SimulateTransactionCost? cost,
        string[]? events, long? latestLedger, uint? minResourceFee, RestorePreamble? restorePreambleInfo,
        SimulateInvokeHostFunctionResult[]? results)
    {
        _transactionData = transactionData;
        Error = error;
        Cost = cost;
        Events = events;
        LatestLedger = latestLedger;
        MinResourceFee = minResourceFee;
        RestorePreambleInfo = restorePreambleInfo;
        Results = results;
    }

    [Obsolete(
        "The cost object is legacy, inaccurate, and will be deprecated in future RPC releases. Please use SorobanTransactionData to retrieve the correct resources.")]
    public SimulateTransactionCost? Cost { get; }

    /// <summary>
    ///     This field will include details about why the invoke host function call failed.
    /// </summary>
    /// <para>(optional) Only present if the transaction failed.</para>
    public string? Error { get; }

    /// <summary>
    ///     Array of serialized base64 strings - Array of the events emitted during the contract invocation. The events are
    ///     ordered by their emission time. (an array of serialized base64 strings).
    ///     <para>
    ///         Only present when simulating of InvokeHostFunction operations, note that it can be present on error,
    ///         providing extra context about what failed.
    ///     </para>
    /// </summary>
    public string[]? Events { get; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Soroban RPC at the time it handled the request.
    /// </summary>
    public long? LatestLedger { get; }

    /// <summary>
    ///     (Optional) Not present in case of error.
    ///     <para>
    ///         Recommended minimum resource fee to add when submitting the transaction. This fee is to be added on top of the
    ///         Stellar network fee.
    ///     </para>
    ///     See https://developers.stellar.org/docs/encyclopedia/fees-surge-pricing-fee-strategies#network-fees-on-stellar.
    /// </summary>
    public uint? MinResourceFee { get; }

    /// <summary>
    ///     If present, it indicates that the simulation detected archived ledger entries which need to be restored before the
    ///     submission of the <c>InvokeHostFunction</c> operation. The <see cref="MinResourceFee" /> and
    ///     <see cref="SorobanTransactionData" /> fields should be used to submit a transaction containing a
    ///     <c>RestoreFootprint</c> operation.
    ///     <para>
    ///         (optional) It can only be present on successful simulation (i.e. no error) of <c>InvokeHostFunction</c>
    ///         operations.
    ///     </para>
    /// </summary>
    [JsonProperty(PropertyName = "restorePreamble")]
    public RestorePreamble? RestorePreambleInfo { get; } // TODO Unit test

    /// <summary>
    ///     An array of the individual host function call results.
    ///     This will only contain a single element if present, because only a single
    ///     <c>invokeHostFunctionOperation</c> is supported per transaction.
    /// </summary>
    public SimulateInvokeHostFunctionResult[]? Results { get; }

    /// <summary>
    ///     The recommended Soroban Transaction Data to use when submitting the simulated transaction. This data contains the
    ///     refundable fee and resource usage information such as the ledger footprint and IO access data.
    ///     <para>Not present in case of error.</para>
    /// </summary>
    public SorobanTransactionData? SorobanTransactionData =>
        _transactionData != null ? SorobanTransactionData.FromXdrBase64(_transactionData) : null;


    public SorobanAuthorizationEntry[]? SorobanAuthorization
    {
        get
        {
            if (Results is not { Length: > 0 } || Results[0].Auth == null) return null;
            return Results[0].Auth?.Select(SorobanAuthorizationEntry.FromXdrBase64).ToArray();
        }
    }

    /// <summary>
    ///     It can only present on successful simulation (i.e. no error) of <c>InvokeHostFunction</c> operations.
    ///     If present, it indicates the simulation detected expired ledger entries which requires restoring
    ///     with the submission of a <c>RestoreFootprint</c> operation before submitting the <c>InvokeHostFunction</c>
    ///     operation.
    ///     The <c>MinResourceFee</c> and <c>SorobanTransactionData</c> fields should be used to construct the transaction
    ///     containing the
    ///     <c>RestoreFootprint</c> operation.
    /// </summary>
    [JsonObject]
    public class RestorePreamble
    {
        [JsonProperty(PropertyName = "transactionData")]
        private readonly string _transactionData;

        /// <summary>
        ///     Recommended minimum resource fee to add when submitting the <c>RestoreFootprint</c> operation. This fee is to be
        ///     added on
        ///     top of the Stellar network fee.
        /// </summary>
        [JsonProperty(PropertyName = "minResourceFee")]
        public readonly long MinResourceFee;

        public RestorePreamble(string transactionData, long minResourceFee)
        {
            _transactionData = transactionData;
            MinResourceFee = minResourceFee;
        }

        /// <summary>
        ///     The recommended Soroban Transaction Data to use when submitting the <c>RestoreFootprint</c> operation.
        /// </summary>
        public SorobanTransactionData SorobanTransactionData =>
            SorobanTransactionData.FromXdrBase64(_transactionData); //TODO Unit test
    }

    /// <summary>
    ///     Information about the fees expected, instructions used, etc.
    /// </summary>
    [JsonObject]
    public class SimulateTransactionCost
    {
        /// <summary>
        ///     Number of the total cpu instructions consumed by this transaction.
        /// </summary>
        [JsonProperty(PropertyName = "cpuInsns")]
        public readonly long CpuInstructions;

        /// <summary>
        ///     Number of the total memory bytes allocated by this transaction.
        /// </summary>
        [JsonProperty(PropertyName = "memBytes")]
        public readonly long MemoryBytes;
    }

    /// <summary>
    ///     Used as a part of simulate transaction.
    ///     See https://soroban.stellar.org/api/methods/simulateTransaction
    /// </summary>
    [JsonObject]
    public class SimulateInvokeHostFunctionResult
    {
        /// <summary>
        ///     Array of serialized base64 strings - Per-address authorizations recorded when simulating this Host Function call.
        /// </summary>
        [JsonProperty(PropertyName = "auth")] public readonly string[]? Auth;

        /// <summary>
        ///     (optional) Only present on success. xdr-encoded return value of the contract call operation.
        /// </summary>
        [JsonProperty(PropertyName = "xdr")] public readonly string? Xdr; // TODO Unit test on error

        public SimulateInvokeHostFunctionResult(string[]? auth, string? xdr)
        {
            Auth = auth;
            Xdr = xdr;
        }
    }
}