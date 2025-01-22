using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents AccountMerge operation response.
/// </summary>
public class ClaimClaimableBalanceOperationResponse : OperationResponse
{
    public override int TypeId => 15;

    [JsonPropertyName("balance_id")]
    public string BalanceID { get; init; }

    [JsonPropertyName("claimant")]
    public string Claimant { get; init; }

    [JsonPropertyName("claimant_muxed")]
    public string ClaimantMuxed { get; init; }

    [JsonPropertyName("claimant_muxed_id")]
    public ulong? ClaimantMuxedID { get; init; }
}