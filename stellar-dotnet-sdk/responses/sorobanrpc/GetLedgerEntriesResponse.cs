using System.Linq;
using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class GetLedgerEntriesResponse
{
    [JsonProperty(PropertyName = "entries")]
    private readonly LedgerEntryResult[]? _entryResults;

    [JsonProperty(PropertyName = "latestLedger")]
    public readonly uint? LatestLedger;

    public GetLedgerEntriesResponse(LedgerEntryResult[]? entryResults, uint? latestLedger)
    {
        _entryResults = entryResults;
        LatestLedger = latestLedger;
    }

    public LedgerEntry[]? LedgerEntries =>
        _entryResults?.Select(x => x.LedgerEntry).ToArray();

    public LedgerKey[]? LedgerKeys =>
        _entryResults?.Select(x => x.LedgerKey).ToArray();

    /// <summary>
    ///     Represents a single entry fetched from Soroban server by method getLedgerEntries().
    /// </summary>
    /// <seealso href="https://soroban.stellar.org/api/methods/getLedgerEntries" />
    public class LedgerEntryResult
    {
        /// <summary>
        ///     The base-64 encoded XDR string of the key of the ledger entry.
        /// </summary>
        [JsonProperty(PropertyName = "key")] private readonly string _key;

        /// <summary>
        ///     The base-64 encoded XDR string of the <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.
        /// </summary>
        private readonly string _xdr;

        /// <summary>
        ///     The ledger number of the last time this entry was updated (optional).
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedLedgerSeq")]
        public readonly uint LastModifiedLedger;

        /// <summary>
        ///     The ledger sequence number after which the ledger entry would expire. This field exists only for ContractCodeEntry
        ///     and ContractDataEntry ledger entries (optional).
        /// </summary>
        [JsonProperty(PropertyName = "liveUntilLedgerSeq")]
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