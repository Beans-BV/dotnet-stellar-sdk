using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class ClaimableBalanceResponse : Response
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }

    [JsonPropertyName("asset")]
    public string Asset { get; init; }

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; init; }

    [JsonPropertyName("last_modified_ledger")]
    public long LastModifiedLedger { get; init; }

    [JsonPropertyName("claimants")]
    public Claimant[] Claimants { get; init; }
}