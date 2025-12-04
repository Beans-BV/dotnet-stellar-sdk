using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class FriendBotResponse
{
    [JsonPropertyName("_links")]
    public FriendBotResponseLinks Links { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("extras")]
    public SubmitTransactionResponse.Extras Extras { get; init; }

    [JsonPropertyName("detail")]
    public string Detail { get; init; }

    [JsonPropertyName("hash")]
    public string Hash { get; init; }

    [JsonPropertyName("ledger")]
    public long Ledger { get; init; }

    [JsonPropertyName("envelope_xdr")]
    public string EnvelopeXdr { get; init; }

    [JsonPropertyName("result_xdr")]
    public string ResultXdr { get; init; }

    [JsonPropertyName("result_meta_xdr")]
    public string ResultMetaXdr { get; init; }

    public class FriendBotResponseLinks
    {
        [JsonPropertyName("transaction")]
        public Link<TransactionResponse> Transaction { get; init; }
    }
}