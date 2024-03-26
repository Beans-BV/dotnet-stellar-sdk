using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

public class FriendBotResponseLinks
{
    public FriendBotResponseLinks(Link<TransactionResponse> transaction)
    {
        Transaction = transaction;
    }

    [JsonProperty(PropertyName = "transaction")]
    public Link<TransactionResponse> Transaction { get; private set; }
}