using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class ClaimClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 15;

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceID { get; init; }

    [JsonProperty(PropertyName = "claimant")]
    public string Claimant { get; init; }

    [JsonProperty(PropertyName = "claimant_muxed")]
    public string ClaimantMuxed { get; init; }

    [JsonProperty(PropertyName = "claimant_muxed_id")]
    public ulong? ClaimantMuxedID { get; init; }
}