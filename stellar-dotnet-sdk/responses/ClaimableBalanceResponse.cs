using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

[JsonObject]
public class ClaimableBalanceResponse : Response
{
    public ClaimableBalanceResponse(string id, string pagingToken, string asset, string amount, string sponsor,
        long lastModifiedLedger, Claimant[] claimants)
    {
        Id = id;
        PagingToken = pagingToken;
        Asset = asset;
        Amount = amount;
        Sponsor = sponsor;
        LastModifiedLedger = lastModifiedLedger;
        Claimants = claimants;
    }

    [JsonProperty(PropertyName = "id")] public string Id { get; private set; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; private set; }

    [JsonProperty(PropertyName = "asset")] public string Asset { get; private set; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; private set; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; private set; }

    [JsonProperty(PropertyName = "last_modified_ledger")]
    public long LastModifiedLedger { get; private set; }

    [JsonProperty(PropertyName = "claimants")]
    public Claimant[] Claimants { get; private set; }
}