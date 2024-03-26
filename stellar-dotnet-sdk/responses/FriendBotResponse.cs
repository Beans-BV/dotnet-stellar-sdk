using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

[JsonObject]
public class FriendBotResponse
{
    public FriendBotResponse(FriendBotResponseLinks links, string type, string title, string status,
        SubmitTransactionResponse.Extras extras, string detail, string hash, string ledger, string envelopeXdr,
        string resultXdr, string resultMetaXdr)
    {
        Links = links;
        Type = type;
        Title = title;
        Status = status;
        Extras = extras;
        Detail = detail;
        Hash = hash;
        Ledger = ledger;
        EnvelopeXdr = envelopeXdr;
        ResultXdr = resultXdr;
        ResultMetaXdr = resultMetaXdr;
    }

    [JsonProperty(PropertyName = "_links")]
    public FriendBotResponseLinks Links { get; set; }

    [JsonProperty(PropertyName = "type")] public string Type { get; set; }

    [JsonProperty(PropertyName = "title")] public string Title { get; set; }

    [JsonProperty(PropertyName = "status")]
    public string Status { get; set; }

    [JsonProperty(PropertyName = "extras")]
    public SubmitTransactionResponse.Extras Extras { get; set; }

    [JsonProperty(PropertyName = "detail")]
    public string Detail { get; set; }

    [JsonProperty(PropertyName = "hash")] public string Hash { get; set; }

    [JsonProperty(PropertyName = "ledger")]
    public string Ledger { get; set; }

    [JsonProperty(PropertyName = "envelope_xdr")]
    public string EnvelopeXdr { get; private set; }

    [JsonProperty(PropertyName = "result_xdr")]
    public string ResultXdr { get; private set; }

    [JsonProperty(PropertyName = "result_meta_xdr")]
    public string ResultMetaXdr { get; private set; }
}