using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_sponsorship_updated effect response.
/// </summary>
public class ClaimableBalanceSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 70;

    [JsonPropertyName("balance_id")]
    public string BalanceId { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonPropertyName("new_sponsor")]
    public string NewSponsor { get; init; }
}