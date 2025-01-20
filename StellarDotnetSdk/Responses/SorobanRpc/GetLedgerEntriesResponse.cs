﻿using System.Linq;
using System.Text.Json.Serialization;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;

namespace StellarDotnetSdk.Responses.SorobanRpc;

public class GetLedgerEntriesResponse
{
    public GetLedgerEntriesResponse(LedgerEntryResult[]? entryResults, uint? latestLedger)
    {
        EntryResults = entryResults;
        LatestLedger = latestLedger;
    }

    [JsonPropertyName("entries")]
    private LedgerEntryResult[]? EntryResults { get; }

    public uint? LatestLedger { get; }

    public LedgerEntry[]? LedgerEntries =>
        EntryResults?.Select(x => x.LedgerEntry).ToArray();

    public LedgerKey[]? LedgerKeys =>
        EntryResults?.Select(x => x.LedgerKey).ToArray();

    /// <summary>
    ///     Represents a single entry fetched from Soroban server by method getLedgerEntries().
    /// </summary>
    /// <seealso href="https://developers.stellar.org/docs/data/rpc/api-reference/methods/getLedgerEntries" />
    public class LedgerEntryResult
    {
        /// <summary>
        ///     The base-64 encoded XDR string of the key of the ledger entry.
        /// </summary>
        [JsonPropertyName("key")] private readonly string _key;

        /// <summary>
        ///     The base-64 encoded XDR string of the <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.
        /// </summary>
        private readonly string _xdr;

        /// <summary>
        ///     The ledger number of the last time this entry was updated (optional).
        /// </summary>
        [JsonPropertyName("lastModifiedLedgerSeq")]
        public readonly uint LastModifiedLedger;

        /// <summary>
        ///     The ledger sequence number after which the ledger entry would expire. This field exists only for ContractCodeEntry
        ///     and ContractDataEntry ledger entries (optional).
        /// </summary>
        [JsonPropertyName("liveUntilLedgerSeq")]
        public readonly uint LiveUntilLedger;

        public LedgerEntryResult(string key, uint lastModifiedLedger, uint liveUntilLedger, string xdr)
        {
            _key = key;
            LastModifiedLedger = lastModifiedLedger;
            LiveUntilLedger = liveUntilLedger;
            _xdr = xdr;
        }

        /// <summary>
        ///     The corresponding <see cref="LedgerEntry" /> object constructed based on this response.
        /// </summary>
        public LedgerEntry LedgerEntry
        {
            get
            {
                var ledgerEntry = LedgerEntry.FromXdrBase64(_xdr);
                ledgerEntry.LastModifiedLedgerSeq = LastModifiedLedger;
                ledgerEntry.LiveUntilLedger = LiveUntilLedger;
                return ledgerEntry;
            }
        }

        /// <summary>
        ///     The ledger key object corresponding to the ledger entry.
        /// </summary>
        public LedgerKey LedgerKey => LedgerKey.FromXdrBase64(_key);
    }
}