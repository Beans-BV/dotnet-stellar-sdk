using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
/// <summary>
///     Represents claimable_balance_sponsorship_created effect response.
/// </summary>
public class ClaimableBalanceSponsorshipCreatedEffectResponse : EffectResponse
{
    public override int TypeId => 69;

    [JsonProperty(PropertyName = "balance_id")]
    public string BalanceId { get; init; }

    [JsonProperty(PropertyName = "sponsor")]
    public string Sponsor { get; init; }
}