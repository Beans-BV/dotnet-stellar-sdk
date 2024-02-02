namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class GetEventsResponse
{
    public EventInfo[]? Events;
    public long? LatestLedger;

    public class EventInfo
    {
        public string ContractId;

        public string Id;

        public bool InSuccessfulContractCall;

        public int Ledger;

        public string LedgerClosedAt;

        public string PagingToken;

        public string[] Topic;
        public string Type;

        public string Value;
    }
}