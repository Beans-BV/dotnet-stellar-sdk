using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_sponsorship_removed effect response.
/// </summary>
public class ClaimableBalanceSponsorshipRemovedEffectResponse : EffectResponse
{
    public override int TypeId => 71;

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }
}