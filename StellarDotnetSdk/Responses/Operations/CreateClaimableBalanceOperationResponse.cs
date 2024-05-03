using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class CreateClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 14;

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }

    [JsonProperty(PropertyName = "asset")] public string Asset { get; init; }

    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    [JsonProperty(PropertyName = "claimants")]
    public Claimant[] Claimants { get; init; }
}