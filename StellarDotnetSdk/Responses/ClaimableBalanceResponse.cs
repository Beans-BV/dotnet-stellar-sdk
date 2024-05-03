using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class ClaimableBalanceResponse : Response
{
    [JsonProperty(PropertyName = "id")] public string Id { get; init; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; init; }

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }

    [JsonProperty(PropertyName = "last_modified_ledger")]
    public long LastModifiedLedger { get; init; }

    [JsonProperty(PropertyName = "claimants")]
    public Claimant[] Claimants { get; init; }
}