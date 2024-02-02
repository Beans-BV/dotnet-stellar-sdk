using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class GetLedgerEntriesResponse
{
    public LedgerEntryResult?[] Entries;
    public long LatestLedger;

    public class LedgerEntryResult
    {
        public string Key;

        [JsonProperty(PropertyName = "lastModifiedLedgerSeq")]
        public long LastModifiedLedger;

        [JsonProperty(PropertyName = "liveUntilLedgerSeq")]
        public long LiveUntilLedger;

        public string Xdr;
    }
}