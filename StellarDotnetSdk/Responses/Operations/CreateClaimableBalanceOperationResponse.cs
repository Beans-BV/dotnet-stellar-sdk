using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class CreateClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 14;

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; init; }

    [JsonPropertyName("asset")]
    public string Asset { get; init; }

    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    [JsonPropertyName("claimants")]
    public Claimant[] Claimants { get; init; }
}