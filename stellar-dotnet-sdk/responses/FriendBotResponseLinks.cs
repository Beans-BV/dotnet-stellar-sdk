using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

public class FriendBotResponseLinks
{
    public FriendBotResponseLinks(Link<TransactionResponse> transaction)
    {
        Transaction = transaction;
    }

    [JsonProperty(PropertyName = "transaction")]
    public Link<TransactionResponse> Transaction { get; private set; }
}