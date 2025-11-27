using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_sponsorship_removed effect response.
/// </summary>
public class ClaimableBalanceSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 71;

    [JsonPropertyName("balance_id")]
    public string BalanceId { get; init; }

    [JsonPropertyName("former_sponsor")]
    public string FormerSponsor { get; init; }
}