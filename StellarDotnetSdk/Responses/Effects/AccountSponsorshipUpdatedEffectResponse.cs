using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable
/// <summary>
///     Represents account_sponsorship_updated effect response.
/// </summary>
public class AccountSponsorshipUpdatedEffectResponse : EffectResponse
{
    public override int TypeId => 61;


    [JsonProperty(PropertyName = "former_sponsor")]
    public string FormerSponsor { get; init; }

    [JsonProperty(PropertyName = "new_sponsor")]
    public string NewSponsor { get; init; }
}