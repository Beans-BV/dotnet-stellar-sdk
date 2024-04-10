using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Represents claimable_balance_sponsorship_updated effect response.
/// </summary>
public class ClaimableBalanceSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 70;

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }

    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonProperty(PropertyName = "new_sponsor")]
    public string NewSponsor { get; init; }
}