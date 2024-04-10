using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class FriendBotResponse
{
    [JsonProperty(PropertyName = "_links")]
    public FriendBotResponseLinks Links { get; init; }

    [JsonProperty(PropertyName = "type")] public string Type { get; init; }

    [JsonProperty(PropertyName = "title")] public string Title { get; init; }

    [JsonProperty(PropertyName = "status")]
    public string Status { get; init; }

    [JsonProperty(PropertyName = "extras")]
    public SubmitTransactionResponse.Extras Extras { get; init; }

    [JsonProperty(PropertyName = "detail")]
    public string Detail { get; init; }

    [JsonProperty(PropertyName = "hash")] public string Hash { get; init; }

    [JsonProperty(PropertyName = "ledger")]
    public string Ledger { get; init; }

    [JsonProperty(PropertyName = "envelope_xdr")]
    public string EnvelopeXdr { get; init; }

    [JsonProperty(PropertyName = "result_xdr")]
    public string ResultXdr { get; init; }

    [JsonProperty(PropertyName = "result_meta_xdr")]
    public string ResultMetaXdr { get; init; }

    public class FriendBotResponseLinks
    {
        [JsonProperty(PropertyName = "transaction")]
        public Link<TransactionResponse> Transaction { get; init; }
    }
}